using Game.Core;
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
    /// This one class replaces LaserPoolManager and BoomerangPoolManager, which were the
    /// same 110 lines twice over. Everything that used to be duplicated - the null checks,
    /// the container creation, the event hook-up, the forwarding - is written here once.
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
    public abstract class ProjectilePoolManager<TProjectile> : MonoBehaviour, IObjectPool<TProjectile>
        where TProjectile : BaseProjectile
    {
        [Header("What to pool")]
        // FormerlySerializedAs keeps the values already wired on Prefab_Mario: the field
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
                 "root of the scene, which is what you want: the container must never move.")]
        [SerializeField] private Transform container;

        [Header("Pool size")]
        [Tooltip("Created during loading, so the first shot costs nothing")]
        [SerializeField] private int prewarmCount = 5;

        [SerializeField] private int maxSize = 5;

        [Tooltip("May the pool create more than it prewarmed, up to Max Size?")]
        [SerializeField] private bool allowGrowth;

        [Header("Diagnostics")]
        [Tooltip("Log every take and every return. Handy while balancing, noisy otherwise.")]
        [SerializeField] private bool logTraffic = true;

        private GenericObjectPool<TProjectile> _pool;
        private string _logPrefix;

        /// <summary>Prefix for this pool's console messages, e.g. "[LaserProjectile]".</summary>
        protected string LogPrefix
        {
            get
            {
                if (_logPrefix == null)
                    _logPrefix = "[" + typeof(TProjectile).Name + "]";

                return _logPrefix;
            }
        }

        public int CountInactive { get { return _pool != null ? _pool.CountInactive : 0; } }

        // Private on purpose: a subclass that declared its own Awake would silently replace
        // this one and the pool would never be built. Subclasses override OnPoolReady().
        private void Awake()
        {
            if (prefab == null || config == null)
            {
                Debug.LogError(LogPrefix + " pool manager needs both a prefab and a Projectile Config asset.", this);
                return;
            }

            Transform parent = container != null ? container : CreateRootContainer();

            // The only place the concrete types are named. Everything downstream talks
            // through interfaces: the director sees IProjectileBuilder, the pool sees
            // IFactory, the weapon sees IObjectPool.
            ProjectileBuilder<TProjectile> builder = new ProjectileBuilder<TProjectile>(prefab, parent);
            ProjectileDirector<TProjectile> director = new ProjectileDirector<TProjectile>(builder);
            ConfiguredProjectileFactory<TProjectile> factory = new ConfiguredProjectileFactory<TProjectile>(director, config);

            _pool = new GenericObjectPool<TProjectile>(factory, prewarmCount, maxSize, allowGrowth);

            // Logged from here, not from inside the generic pool: the pool must stay
            // projectile-agnostic, but the console messages the exercise asks for are not.
            _pool.ItemTaken += OnItemTaken;
            _pool.ItemReleased += OnItemReleased;

            Debug.Log(LogPrefix + " pool prewarmed with " + _pool.CountInactive + " item(s)", this);

            OnPoolReady();
        }

        /// <summary>Subclass hook, run once the pool exists. Nothing needs it today.</summary>
        protected virtual void OnPoolReady() { }

        /// <summary>
        /// Parks the pooled objects on their own object at the root of the scene.
        ///
        /// This matters more than it looks. If the container were this object - and this
        /// component usually sits on Mario - then every sleeping projectile would be a
        /// child of Mario and would be dragged around by him. The hierarchy stays flat and
        /// still, so Unity never recalculates those transforms.
        /// </summary>
        private Transform CreateRootContainer()
        {
            GameObject holder = new GameObject(typeof(TProjectile).Name + "Pool");
            holder.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return holder.transform;
        }

        private void OnDestroy()
        {
            if (_pool == null)
                return;

            _pool.ItemTaken -= OnItemTaken;
            _pool.ItemReleased -= OnItemReleased;
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

        /// <summary>Overridable so a weapon-specific message can replace the generic one.</summary>
        protected virtual void OnItemTaken(TProjectile item)
        {
            if (logTraffic)
                Debug.Log(LogPrefix + " taken from pool (inactive left: " + CountInactive + ")");
        }

        protected virtual void OnItemReleased(TProjectile item)
        {
            if (logTraffic)
                Debug.Log(LogPrefix + " returned to pool (inactive now: " + CountInactive + ")");
        }
    }
}
