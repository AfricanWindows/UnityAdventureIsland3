using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// COMPOSITION ROOT for the boomerang - the exact counterpart of LaserPoolManager. It
    /// builds the chain builder -> director -> factory -> pool once in Awake, then only
    /// forwards calls.
    ///
    /// The pool is sized to ONE, and that single pooled boomerang IS the one round of
    /// ammo. Throwing empties the pool, so Get() returns null until the boomerang finishes
    /// its loop and comes home. (BoomerangWeapon also checks IsFlying, so the rule holds
    /// even if this size is ever raised.)
    /// </summary>
    public class BoomerangPoolManager : MonoBehaviour, IObjectPool<BoomerangProjectile>
    {
        [Header("What to pool")]
        [SerializeField] private BoomerangProjectile boomerangPrefab;

        [Tooltip("Shared Weapons/Projectile Config asset. Lifetime = one loop's duration; " +
                 "keep Pierces Enemies ON so a hit does not cut the loop short.")]
        [SerializeField] private ProjectileConfigSO boomerangConfig;

        [Tooltip("Parent for the pooled boomerang. Leave empty and one is created at the " +
                 "root of the scene, so the sleeping boomerang is never dragged around by Mario.")]
        [SerializeField] private Transform container;

        [Header("Pool size")]
        [Tooltip("Keep at 1: the single boomerang is the one round of ammo.")]
        [SerializeField] private int prewarmCount = 1;

        [SerializeField] private int maxSize = 1;

        [Tooltip("Leave OFF: a second boomerang would break the one-at-a-time rule.")]
        [SerializeField] private bool allowGrowth;

        private GenericObjectPool<BoomerangProjectile> _pool;

        public int CountInactive { get { return _pool != null ? _pool.CountInactive : 0; } }

        private void Awake()
        {
            if (boomerangPrefab == null || boomerangConfig == null)
            {
                Debug.LogError("[Boomerang] BoomerangPoolManager needs both a Boomerang Prefab and a Projectile Config asset.", this);
                return;
            }

            Transform parent = container != null ? container : CreateRootContainer();

            // The only place the concrete types are named. Everything downstream is interfaces.
            BoomerangBuilder builder = new BoomerangBuilder(boomerangPrefab, parent);
            BoomerangDirector director = new BoomerangDirector(builder);
            BoomerangFactory factory = new BoomerangFactory(director, boomerangConfig);

            _pool = new GenericObjectPool<BoomerangProjectile>(factory, prewarmCount, maxSize, allowGrowth);

            _pool.ItemTaken += OnBoomerangTaken;
            _pool.ItemReleased += OnBoomerangReleased;

            Debug.Log("[Boomerang] Pool prewarmed with " + _pool.CountInactive + " boomerang(s)", this);
        }

        private Transform CreateRootContainer()
        {
            GameObject holder = new GameObject("BoomerangPool");
            holder.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return holder.transform;
        }

        private void OnDestroy()
        {
            if (_pool == null)
                return;

            _pool.ItemTaken -= OnBoomerangTaken;
            _pool.ItemReleased -= OnBoomerangReleased;
        }

        /// <summary>The boomerang, or null while the one round is still out on its loop.</summary>
        public BoomerangProjectile Get()
        {
            return _pool != null ? _pool.Get() : null;
        }

        /// <summary>
        /// Normally never called by hand: the boomerang returns itself through the callback
        /// the pool gave it. Here so the pool stays usable through IObjectPool.
        /// </summary>
        public void Release(BoomerangProjectile item)
        {
            if (_pool != null)
                _pool.Release(item);
        }

        private void OnBoomerangTaken(BoomerangProjectile boomerang)
        {
            Debug.Log("[Boomerang] Thrown - no round left until it returns");
        }

        private void OnBoomerangReleased(BoomerangProjectile boomerang)
        {
            Debug.Log("[Boomerang] Returned to pool - ready to throw again");
        }
    }
}
