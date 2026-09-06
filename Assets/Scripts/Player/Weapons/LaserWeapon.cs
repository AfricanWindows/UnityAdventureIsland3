using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The laser gun. It decides WHEN a laser may leave the barrel and where it starts -
    /// nothing else. It does not build lasers, does not own them, and does not destroy
    /// them; it borrows one and hands it back by itself (Single Responsibility).
    ///
    /// The field is a LaserPoolManager because the Inspector cannot serialise an interface,
    /// but from Awake on this class only ever talks to IObjectPool&lt;LaserProjectile&gt;.
    /// Swapping in a different pool - or a fake one in a test - needs no change here
    /// (Dependency Inversion).
    ///
    /// WeaponsHandler picks it up automatically through IWeapon, so adding the laser to
    /// Mario needed no edit in the handler at all (Open/Closed).
    /// </summary>
    public sealed class LaserWeapon : BaseWeapon
    {
        [Tooltip("The pool that hands out lasers. Drag the LaserPool object here.")]
        [SerializeField] private LaserPoolManager laserPool;

        [Tooltip("Where a laser appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        // Resolved once in Awake. Neither of these is looked up while shooting.
        private IObjectPool<LaserProjectile> _pool;
        private Transform _firePoint;

        public override WeaponType Type { get { return WeaponType.Laser; } }

        protected override void OnAwake()
        {
            _pool = laserPool;
            _firePoint = firePoint != null ? firePoint : transform;

            if (_pool == null)
                Debug.LogError("[Laser] LaserWeapon has no LaserPoolManager assigned.", this);
        }

        protected override bool FireInternal()
        {
            if (_pool == null)
                return false;

            LaserProjectile laser = _pool.Get();

            if (laser == null)
            {
                // Not a warning: with a fixed-size pool this is the ammo limit doing its
                // job. Nothing is fired until one of the lasers in the air comes back.
                Debug.Log("[Laser] Pool exhausted - all lasers are still in the air");
                return false;
            }

            // The weapon says "go", the projectile decides how. Straight up, in this case.
            laser.Fire(_firePoint.position);
            return true;
        }
    }
}
