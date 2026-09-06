using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// COMPOSITION ROOT for the laser: the one place that knows which prefab, which config
    /// and which container belong together. It builds the chain
    /// builder -> director -> factory -> pool once, in Awake, and then does nothing but
    /// forward calls.
    ///
    /// Note what is NOT here: no queue, no reuse logic, no Instantiate. All of that lives
    /// in GenericObjectPool, which is a plain C# class and knows nothing about lasers.
    /// A MonoBehaviour is needed only to hold Inspector references and to exist in a scene
    /// (Single Responsibility).
    ///
    /// Weapons receive this through a [SerializeField] reference, not a static Instance -
    /// which is why LaserWeapon depends on IObjectPool&lt;LaserProjectile&gt; and could be
    /// handed a test pool tomorrow.
    /// </summary>
    public class LaserPoolManager : MonoBehaviour, IObjectPool<LaserProjectile>
    {
        [Header("What to pool")]
        [SerializeField] private LaserProjectile laserPrefab;
        [SerializeField] private ProjectileConfigSO laserConfig;

        [Tooltip("Parent for every pooled laser. Leave empty and one is created at the root " +
                 "of the scene, which is what you want: the container must never move.")]
        [SerializeField] private Transform container;

        [Header("Pool size")]
        [Tooltip("Created during loading, so the first shot costs nothing")]
        [SerializeField] private int prewarmCount = 5;

        [SerializeField] private int maxSize = 5;

        [Tooltip("May the pool create more than it prewarmed, up to Max Size?")]
        [SerializeField] private bool allowGrowth;

        private GenericObjectPool<LaserProjectile> _pool;

        public int CountInactive { get { return _pool != null ? _pool.CountInactive : 0; } }

        private void Awake()
        {
            if (laserPrefab == null || laserConfig == null)
            {
                Debug.LogError("[Laser] LaserPoolManager needs both a Laser Prefab and a Laser Config asset.", this);
                return;
            }

            Transform parent = container != null ? container : CreateRootContainer();

            // The only place the concrete types are named. Everything downstream talks
            // through interfaces.
            LaserBuilder builder = new LaserBuilder(laserPrefab, parent);
            LaserDirector director = new LaserDirector(builder);
            LaserFactory factory = new LaserFactory(director, laserConfig);

            _pool = new GenericObjectPool<LaserProjectile>(factory, prewarmCount, maxSize, allowGrowth);

            // Logged from here, not from inside the generic pool: the pool must stay
            // laser-agnostic, but the console messages the exercise asks for are laser talk.
            _pool.ItemTaken += OnLaserTaken;
            _pool.ItemReleased += OnLaserReleased;

            Debug.Log("[Laser] Pool prewarmed with " + _pool.CountInactive + " lasers", this);
        }

        /// <summary>
        /// Parks the pooled lasers on their own object at the root of the scene.
        ///
        /// This matters more than it looks. If the container were this object - and this
        /// component usually sits on Mario - then every sleeping laser would be a child of
        /// Mario and would be dragged around by him. The hierarchy stays flat and still,
        /// so Unity never recalculates those transforms.
        /// </summary>
        private Transform CreateRootContainer()
        {
            GameObject holder = new GameObject("LaserPool");
            holder.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return holder.transform;
        }

        private void OnDestroy()
        {
            if (_pool == null)
                return;

            _pool.ItemTaken -= OnLaserTaken;
            _pool.ItemReleased -= OnLaserReleased;
        }

        /// <summary>An active laser, or null when the pool is empty and may not grow.</summary>
        public LaserProjectile Get()
        {
            return _pool != null ? _pool.Get() : null;
        }

        /// <summary>
        /// Normally never called by hand: a laser returns itself through the callback the
        /// pool gave it. This exists so the pool stays usable through IObjectPool.
        /// </summary>
        public void Release(LaserProjectile item)
        {
            if (_pool != null)
                _pool.Release(item);
        }

        private void OnLaserTaken(LaserProjectile laser)
        {
            Debug.Log("[Laser] Taken from pool (inactive left: " + _pool.CountInactive + ")");
        }

        private void OnLaserReleased(LaserProjectile laser)
        {
            Debug.Log("[Laser] Returned to pool (inactive now: " + _pool.CountInactive + ")");
        }
    }
}
