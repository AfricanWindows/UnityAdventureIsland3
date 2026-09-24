using Game.Core.Controls;
using Game.Projectiles;
using Game.Weapons;
using UnityEngine;

namespace Game.Core.DI
{
    /// <summary>
    /// THE COMPOSITION ROOT for everything SHARED: every service that more than one object
    /// depends on is created here, and here is where each of those abstractions is married to
    /// a concrete implementation. Swapping the keyboard for a gamepad, or the real health view
    /// for a silent one, is an edit in THIS FILE and nowhere else (Open/Closed, Dependency
    /// Inversion) - because no one else ever names those classes.
    ///
    /// It is NOT the only "new" in the project, and claiming that would be a lie worth
    /// spotting. Two kinds of object legitimately build their own:
    ///   - a PRIVATE dependency nobody else can see - PowerController creates its own
    ///     PowerModel and PowerDrainService, because no second object may share that bar;
    ///   - a LOCAL composition root - each ProjectilePoolManager wires its own
    ///     builder -&gt; director -&gt; factory -&gt; pool chain from its own Inspector fields,
    ///     which is the one place where that prefab, that config and that pool belong together.
    /// What all of them have in common is the rule this class exists to protect: the OWNER
    /// may name a concrete type, and nobody else may.
    ///
    /// Why DefaultExecutionOrder instead of the Script Execution Order window: the order
    /// requirement is a property of this class, not of the project, so it belongs in the
    /// class. It also survives a fresh clone of the repository, which a project setting a
    /// teammate forgets to commit does not.
    ///
    /// Awake here runs before every other Awake, so an IInjectable is fully injected by the
    /// time its own Awake runs - which is why the components below can cache their services
    /// in Inject() and simply use them afterwards.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public class GameInstaller : MonoBehaviour
    {
        [Header("Scene-bound services")]
        [Tooltip("The health label. Optional - leave empty and the installer looks it up " +
                 "once, here, which is the one place in the project allowed to search the scene.")]
        [SerializeField] private PlayerHealthView healthView;

        [Tooltip("The power bar. Optional - same rule as the health label above.")]
        [SerializeField] private PowerBarView powerBar;

        [Tooltip("The \"Fruits: 0/20\" label. Optional - found automatically.")]
        [SerializeField] private FruitCounterView fruitCounter;

        [Tooltip("The pool every shooting enemy borrows its shots from. Optional - found automatically.")]
        [SerializeField] private EnemyProjectilePoolManager enemyShotPool;

        [Tooltip("The axe pool. Optional - found automatically, wherever it sits in the scene.")]
        [SerializeField] private AxePoolManager axePool;

        [Tooltip("The boomerang pool. Optional - found automatically.")]
        [SerializeField] private BoomerangPoolManager boomerangPool;

        [Tooltip("The pool both shooting animals borrow from. Optional - found automatically.")]
        [SerializeField] private AnimalShotPoolManager animalShotPool;

        [Tooltip("The object that switches the levels. Optional - found automatically.")]
        [SerializeField] private LevelFlowController levelFlow;

        [Tooltip("Tag the player object carries. The camera and the level flow find him by it.")]
        [SerializeField] private string playerTag = "Player";

        [Header("Diagnostics")]
        [Tooltip("Log every registration and every injected component on start-up.")]
        [SerializeField] private bool verbose = true;

        // Held as IServiceContainer, the WRITING half: this class fills the container and is
        // the only one allowed to. The concrete ServiceContainer is named exactly once, on the
        // right of this line; everything else receives it as the reading half, IServiceResolver.
        private readonly IServiceContainer _container = new ServiceContainer();

        private void Awake()
        {
            RegisterServices();
            InjectSceneObjects();
        }

        /// <summary>
        /// The wiring table. Read top to bottom, this is the complete list of what the game
        /// depends on and what currently satisfies it.
        /// </summary>
        private void RegisterServices()
        {
            // Input: the abstraction is IInputSource, today's implementation is a keyboard.
            _container.Register<IInputSource>(new KeyboardInputSource());

            // The three HUD views live in the UI canvas: scene objects that no player prefab
            // can hold a reference to, so the composition root resolves them once. Doing it
            // HERE is what lets the controllers stop calling FindFirstObjectByType - a Service
            // Locator hidden in a controller.
            RegisterSceneService<IPlayerHealthView, PlayerHealthView>(healthView, "the lives will not be shown");
            RegisterSceneService<IPowerView, PowerBarView>(powerBar, "the power bar will not be drawn");
            RegisterSceneService<IFruitCounterView, FruitCounterView>(fruitCounter, "the fruit count will not be shown");

            // Who the player is. Found ONCE, by tag, instead of by every class that
            // wants him calling FindGameObjectWithTag in its own Update.
            _container.Register<IPlayerProvider>(new TaggedPlayerProvider(playerTag));

            // Resetting the game: the scene-wide search lives in a service of its own,
            // so the level flow only decides WHEN to reset, never HOW to find what to reset.
            _container.Register<IResetService>(new SceneResetService());

            // Every pool in the game, published under the interface its users ask for.
            // Four lines instead of four copies of the same eight - the repetition moved
            // into RegisterPool below, where it is written once (Don't Repeat Yourself).
            //
            // This is also what frees a pool from the player prefab. A prefab cannot hold
            // a reference to a scene object, so the axe and the boomerang used to need
            // their pools ON the player. Registered here, a pool can sit on any object in
            // the scene and the weapons still find it - without naming it.
            RegisterPool<EnemyProjectilePoolManager, EnemyProjectile>(enemyShotPool, "shooting enemies");
            RegisterPool<AxePoolManager, ProjectileAxe>(axePool, "the axe");
            RegisterPool<BoomerangPoolManager, BoomerangProjectile>(boomerangPool, "the boomerang");
            RegisterPool<AnimalShotPoolManager, AnimalShot>(animalShotPool, "the shooting animals");

            // The game's course: which level runs, and what a restart means. ONE object,
            // published under two names - the commands (ILevelFlow) for the door and the
            // restart button, the news (ILevelEvents) for the panels and the pools - so
            // each client sees only the half it uses (Interface Segregation).
            LevelFlowController flow = RegisterSceneService<ILevelFlow, LevelFlowController>(
                levelFlow, "levels will not switch");

            if (flow != null)
                _container.Register<ILevelEvents>(flow);

            if (verbose)
                Debug.Log("[DI] Services registered.", this);
        }

        /// <summary>
        /// Publishes one pool under IObjectPool&lt;TProjectile&gt; - the only face of a pool
        /// that anything else in the game is allowed to see (Interface Segregation).
        ///
        /// GENERIC because the four pools differ in exactly two things: what they hand
        /// out, and which manager holds them. Both are type arguments, so each pool is one
        /// line at the call site: the find and the warning are RegisterSceneService's, the
        /// pool-specific log is written here, once. A fifth pool is one more line at the
        /// call site and no new code here (Open/Closed).
        ///
        /// TManager is a type argument rather than a search for the abstract base class on
        /// purpose: Unity's find is given the exact concrete component, which is what it
        /// was always given before this method existed.
        ///
        /// Nothing that uses a pool ever appears in this signature - a weapon and a snake
        /// are unknown here, and the pool is unknown to them (Dependency Inversion).
        /// </summary>
        /// <param name="assigned">The Inspector reference, or null to search the scene.</param>
        /// <param name="users">Who goes without if it is missing, for the warning.</param>
        private void RegisterPool<TManager, TProjectile>(TManager assigned, string users)
            where TManager : ProjectilePoolManager<TProjectile>
            where TProjectile : BaseProjectile
        {
            // Registered as the INTERFACE. A weapon therefore never learns which manager,
            // or which object in the scene, its projectiles came from.
            TManager pool = RegisterSceneService<IObjectPool<TProjectile>, TManager>(
                assigned, users + " will not fire");

            if (pool != null && verbose)
                Debug.Log("[DI] " + typeof(TProjectile).Name + " pool registered from '" +
                          pool.name + "'.", this);
        }

        /// <summary>
        /// Publishes one scene object under the interface its users ask for: the one
        /// dragged into the Inspector, or else the first one found in the scene.
        ///
        /// GENERIC over both sides of the registration - what the object IS (TComponent,
        /// needed for Unity's search) and what it is published AS (TService, the only face
        /// anyone else sees). The constraint TComponent : TService makes a wrong pairing a
        /// COMPILE error instead of a runtime one. The HUD views, the level flow and every
        /// pool go through here, so "find, warn, register" is written once.
        ///
        /// Include inactive: an object parked on a switched-off parent still has to be
        /// registered, or the first class to ask would find nothing.
        /// </summary>
        /// <param name="assigned">The Inspector reference, or null to search the scene.</param>
        /// <param name="whatBreaks">What goes missing without it, for the warning.</param>
        /// <returns>What was registered, or null when there was nothing to register.</returns>
        private TComponent RegisterSceneService<TService, TComponent>(TComponent assigned, string whatBreaks)
            where TService : class
            where TComponent : Component, TService
        {
            TComponent found = assigned != null
                ? assigned
                : FindAnyObjectByType<TComponent>(FindObjectsInactive.Include);

            if (found == null)
            {
                Debug.LogWarning("[DI] No " + typeof(TComponent).Name + " in the scene - " +
                                 whatBreaks + ".", this);
                return null;
            }

            _container.Register<TService>(found);
            return found;
        }

        /// <summary>
        /// Hands the container to every component in the scene that asked for one.
        ///
        /// The scan happens ONCE, during Awake. That is the trade a composition root exists
        /// to make: one controlled lookup at start-up, so that no gameplay class ever has to
        /// search for anything again. The only other broad searches live in the services
        /// registered above (the player by tag, the restart), never in a gameplay class.
        /// </summary>
        private void InjectSceneObjects()
        {
            // Include inactive objects: a weapon that starts switched off still needs its
            // input source before it is ever enabled.
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

            int injected = 0;

            for (int i = 0; i < all.Length; i++)
            {
                IInjectable injectable = all[i] as IInjectable;

                if (injectable == null)
                    continue;

                injectable.Inject(_container);
                injected++;
            }

            if (verbose)
                Debug.Log("[DI] Injected " + injected + " component(s).", this);
        }
    }
}
