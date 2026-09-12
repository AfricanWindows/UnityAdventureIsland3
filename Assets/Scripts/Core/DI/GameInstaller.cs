using System.Collections.Generic;
using Game.Core.Controls;
using Game.Projectiles;
using Game.Weapons;
using UnityEngine;

namespace Game.Core.DI
{
    /// <summary>
    /// THE COMPOSITION ROOT. The single object in the scene that is allowed to say the
    /// word "new" about a service, and the single place where an abstraction is married to
    /// a concrete implementation.
    ///
    /// Everything else in the game only ever sees interfaces. Swapping the keyboard for a
    /// gamepad, or the real health view for a silent one, is an edit in THIS FILE and
    /// nowhere else (Open/Closed, Dependency Inversion).
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

    [Tooltip("The object that switches the levels. Optional - found automatically.")]
    [SerializeField] private LevelFlowController levelFlow;

    [Tooltip("Tag the player object carries. The camera and the level flow find him by it.")]
    [SerializeField] private string playerTag = "Player";

        [Header("Diagnostics")]
        [Tooltip("Log every registration and every injected component on start-up.")]
        [SerializeField] private bool verbose = true;

        private readonly ServiceContainer _container = new ServiceContainer();

        /// <summary>
        /// Exposed for objects that are created AFTER the scene loaded (a pooled enemy, a
        /// spawned boss). They cannot be injected in Awake because they did not exist yet,
        /// so whoever spawns them calls InjectInto on the new object.
        /// </summary>
        public IServiceContainer Container { get { return _container; } }

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

            // The health label lives in the UI canvas, so it is a scene object rather than
            // something we can "new". Resolving it HERE is what lets PlayerHealthController
            // stop calling FindFirstObjectByType - a Service Locator hidden in a controller.
            PlayerHealthView view = healthView;

            if (view == null)
                view = FindAnyObjectByType<PlayerHealthView>(FindObjectsInactive.Include);

            if (view != null)
                _container.Register<IPlayerHealthView>(view);
            else
                Debug.LogWarning("[DI] No PlayerHealthView in the scene - health will not be displayed.", this);

            // Same story for the power bar: it lives in the UI canvas, so it cannot be
            // dragged onto a player prefab. The composition root resolves it once.
            PowerBarView bar = powerBar;

            if (bar == null)
                bar = FindAnyObjectByType<PowerBarView>(FindObjectsInactive.Include);

            if (bar != null)
                _container.Register<IPowerView>(bar);
            else
                Debug.LogWarning("[DI] No PowerBarView in the scene - the power bar will not be drawn.", this);

            // The fruit label, same story as the two above: a UI object the player
            // prefab cannot hold a reference to.
            FruitCounterView fruits = fruitCounter;

            if (fruits == null)
                fruits = FindAnyObjectByType<FruitCounterView>(FindObjectsInactive.Include);

            if (fruits != null)
                _container.Register<IFruitCounterView>(fruits);
            else
                Debug.LogWarning("[DI] No FruitCounterView in the scene - the fruit count will not be shown.", this);

            // Who the player is. Found ONCE, by tag, instead of by every class that
            // wants him calling FindGameObjectWithTag in its own Update.
            _container.Register<IPlayerProvider>(new TaggedPlayerProvider(playerTag));

            // Every pool in the game, published under the interface its users ask for.
            // Three lines instead of three copies of the same eight - the repetition moved
            // into RegisterPool below, where it is written once (Don't Repeat Yourself).
            //
            // This is also what frees a pool from the player prefab. A prefab cannot hold
            // a reference to a scene object, so the axe and the boomerang used to need
            // their pools ON the player. Registered here, a pool can sit on any object in
            // the scene and the weapons still find it - without naming it.
            RegisterPool<EnemyProjectilePoolManager, EnemyProjectile>(enemyShotPool, "shooting enemies");
            RegisterPool<AxePoolManager, ProjectileAxe>(axePool, "the axe");
            RegisterPool<BoomerangPoolManager, BoomerangProjectile>(boomerangPool, "the boomerang");

            // The game's course: which level runs, and what a restart means.
            LevelFlowController flow = levelFlow;

            if (flow == null)
                flow = FindAnyObjectByType<LevelFlowController>(FindObjectsInactive.Include);

            if (flow != null)
                _container.Register<ILevelFlow>(flow);
            else
                Debug.LogWarning("[DI] No LevelFlowController in the scene - levels will not switch.", this);

            if (verbose)
                Debug.Log("[DI] Services registered.", this);
        }

        /// <summary>
        /// Publishes one pool under IObjectPool&lt;TProjectile&gt; - the only face of a pool
        /// that anything else in the game is allowed to see (Interface Segregation).
        ///
        /// GENERIC because the three pools differ in exactly two things: what they hand
        /// out, and which manager holds them. Both are type arguments, so the find, the
        /// registration, the log and the warning are written ONCE. A fourth pool is one
        /// more line at the call site and no new code here (Open/Closed).
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
            TManager pool = assigned;

            // Include inactive: a pool parked on a switched-off object still has to be
            // registered, or the first weapon to fire would find nothing.
            if (pool == null)
                pool = FindAnyObjectByType<TManager>(FindObjectsInactive.Include);

            if (pool == null)
            {
                Debug.LogWarning("[DI] No " + typeof(TManager).Name + " in the scene - " +
                                 users + " will not fire.", this);
                return;
            }

            // Registered as the INTERFACE. A weapon therefore never learns which manager,
            // or which object in the scene, its projectiles came from.
            _container.Register<IObjectPool<TProjectile>>(pool);

            if (verbose)
                Debug.Log("[DI] " + typeof(TProjectile).Name + " pool registered from '" +
                          pool.name + "'.", this);
        }

        /// <summary>
        /// Hands the container to every component in the scene that asked for one.
        ///
        /// The scan happens ONCE, during Awake, and is the only broad Find in the project.
        /// That is the trade a composition root exists to make: one controlled lookup at
        /// start-up, so that no gameplay class ever has to search for anything again.
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

        /// <summary>
        /// Injects one freshly created object tree. Call this right after Instantiate, or
        /// from a pool factory, so runtime-spawned objects get the same services the scene
        /// objects were given.
        /// </summary>
        public void InjectInto(GameObject root)
        {
            if (root == null)
                return;

            List<IInjectable> found = new List<IInjectable>();
            root.GetComponentsInChildren(true, found);

            for (int i = 0; i < found.Count; i++)
                found[i].Inject(_container);
        }
    }
}
