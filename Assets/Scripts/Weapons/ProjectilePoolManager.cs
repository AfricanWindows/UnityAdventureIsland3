using Game.Core;
using Game.Core.DI;
using Game.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Weapons
{
    /// <summary>
    /// COMPOSITION ROOT for one kind of projectile: the single place that knows which
    /// prefab, which config asset and which container belong together. It builds the chain
    /// builder -&gt; director -&gt; factory -&gt; pool once, in Awake, and afterwards does
    /// nothing but forward calls.
    ///
    /// This one class replaced a hand-written pool manager per weapon, which were the
    /// same 110 lines twice over. Everything that used to be duplicated - the null checks,
    /// the container creation, the forwarding - is written here once.
    ///
    /// Note what is NOT here: no queue, no reuse logic, no Instantiate. All of that lives
    /// in GenericObjectPool, a plain C# class that knows nothing about projectiles. A
    /// MonoBehaviour is needed only to hold Inspector references and to exist in a scene
    /// (Single Responsibility).
    ///
    /// It implements IObjectPool&lt;T&gt;, which is the only thing a weapon is allowed to
    /// see: Get and Release, no prewarm counts, no growth policy (Interface Segregation).
    /// </summary>
    /// <typeparam name="TProjectile">What this pool hands out.</typeparam>
    public abstract class ProjectilePoolManager<TProjectile> : MonoBehaviour, IObjectPool<TProjectile>, IInjectable
        where TProjectile : BaseProjectile
    {
        [Header("What to pool")]
        // FormerlySerializedAs keeps the values already wired on the player prefab: the field
        // used to be called laserPrefab on one manager and boomerangPrefab on the other.
        // Renaming a serialized field without this silently clears the reference.
        [FormerlySerializedAs("laserPrefab")]
        [FormerlySerializedAs("boomerangPrefab")]
        [SerializeField] private TProjectile prefab;

        [Tooltip("The Weapons/Projectile Config asset holding this projectile's numbers.")]
        [FormerlySerializedAs("laserConfig")]
        [FormerlySerializedAs("boomerangConfig")]
        [SerializeField] private ProjectileConfigSO config;

        [Tooltip("Parent for every pooled object. Leave empty and one is created at the " +
                 "root of the scene. If you assign your own, it must sit at the ROOT - a " +
                 "container inside a level is switched off together with that level.")]
        [SerializeField] private Transform container;

        [Header("Pool size")]
        [Tooltip("Created during loading, so the first shot costs nothing")]
        [SerializeField] private int prewarmCount = 5;

        [SerializeField] private int maxSize = 5;

        [Tooltip("May the pool create more than it prewarmed, up to Max Size?")]
        [SerializeField] private bool allowGrowth;

        private GenericObjectPool<TProjectile> _pool;
        private ILevelFlow _flow;
        private string _logPrefix;

        /// <summary>Prefix for this pool's console messages, e.g. "[ProjectileAxe]".</summary>
        protected string LogPrefix
        {
            get
            {
                if (_logPrefix == null)
                    _logPrefix = "[" + typeof(TProjectile).Name + "]";

                return _logPrefix;
            }
        }

        // Private on purpose: a subclass that declared its own Awake would silently replace
        // this one and the pool would never be built. Subclasses override OnPoolReady().
        private void Awake()
        {
            if (prefab == null || config == null)
            {
                Debug.LogError(LogPrefix + " pool manager needs both a prefab and a Projectile Config asset.", this);
                return;
            }

            Transform parent = ResolveContainer();

            // The only place the concrete types are named. Everything downstream talks
            // through interfaces: the director sees IProjectileBuilder, the pool sees
            // IFactory, the weapon sees IObjectPool.
            ProjectileBuilder<TProjectile> builder = new ProjectileBuilder<TProjectile>(prefab, parent);
            ProjectileDirector<TProjectile> director = new ProjectileDirector<TProjectile>(builder);
            ConfiguredProjectileFactory<TProjectile> factory = new ConfiguredProjectileFactory<TProjectile>(director, config);

            _pool = new GenericObjectPool<TProjectile>(factory, prewarmCount, maxSize, allowGrowth);

            OnPoolReady();
        }

        /// <summary>Subclass hook, run once the pool exists. Nothing needs it today.</summary>
        protected virtual void OnPoolReady() { }

        /// <summary>
        /// The container assigned in the Inspector, or a fresh one at the root of the scene.
        ///
        /// The warning is worth more than it looks. The levels are switched with
        /// SetActive, so a container parked inside one of them is switched off with that
        /// level - and a pooled object whose PARENT is inactive stays invisible no matter
        /// what the pool does to the object itself. The weapon would fire, the pool would
        /// report a hand-out, and nothing would appear on screen. Same story, less fatal,
        /// for a container that rides on something that moves: every sleeping projectile
        /// would be dragged along and re-transformed for nothing.
        /// </summary>
        private Transform ResolveContainer()
        {
            if (container == null)
                return CreateRootContainer();

            // The test is "inside a Level", not "has a parent". Parking the pool under a
            // tidy-up object such as Scripts is fine - that object is never switched off.
            // A LEVEL is, every time the player moves on, and an inactive parent takes its
            // sleeping projectiles down with it: the weapon would fire, the pool would
            // report a hand-out, and nothing would appear on screen.
            Level owningLevel = container.GetComponentInParent<Level>(true);

            if (owningLevel != null)
                Debug.LogWarning(LogPrefix + " pool container '" + container.name +
                                 "' sits inside level '" + owningLevel.name + "'. Move it out: " +
                                 "that container is switched off when the level changes, and " +
                                 "every pooled object inside it goes dark with it.", this);

            return container;
        }

        /// <summary>
        /// Parks the pooled objects on their own object at the root of the scene.
        ///
        /// This matters more than it looks. If the container were this object - and this
        /// component usually sits on the player - then every sleeping projectile would be a
        /// child of the player and would be dragged around by him. The hierarchy stays flat and
        /// still, so Unity never recalculates those transforms.
        /// </summary>
        private Transform CreateRootContainer()
        {
            GameObject holder = new GameObject(typeof(TProjectile).Name + "Pool");
            holder.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return holder.transform;
        }

        /// <summary>An active item, or null when the pool is empty and may not grow.</summary>
        public TProjectile Get()
        {
            return _pool != null ? _pool.Get() : null;
        }

        /// <summary>
        /// Normally never called by hand: a projectile returns itself through the callback
        /// the pool gave it. This exists so the pool stays usable through IObjectPool.
        /// </summary>
        public void Release(TProjectile item)
        {
            if (_pool != null)
                _pool.Release(item);
        }

        /// <summary>
        /// Called by GameInstaller. The pool listens to the level flow so that entering a
        /// level - the next one, or level one after a restart - takes back every projectile
        /// still in the air: no shot fired in the old level can hit the player in the new
        /// one. The flow names no pool; it only announces the level (Dependency Inversion).
        /// </summary>
        public void Inject(IServiceContainer container)
        {
            if (container != null && container.TryResolve(out _flow))
                _flow.LevelEntered += ReleaseAllInFlight;
        }

        private void OnDestroy()
        {
            if (_flow != null)
                _flow.LevelEntered -= ReleaseAllInFlight;
        }

        private void ReleaseAllInFlight()
        {
            if (_pool != null)
                _pool.ReleaseAll();
        }
    }
}
