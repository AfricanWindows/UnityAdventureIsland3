using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The fire flower's gun. It decides WHEN a fireball may leave Mario's hand and where
    /// it starts - nothing else. It does not build fireballs, does not own them, and no
    /// longer destroys them; it borrows one and the fireball returns itself.
    ///
    /// It used to be a standalone MonoBehaviour with its own _isEquip flag, its own
    /// Instantiate call and no cooldown. Moving it onto Game.Weapons.BaseWeapon deleted all
    /// of that: the unlock gate, the cooldown and the fixed order of the three checks are
    /// inherited from the Template Method, so this class cannot forget any of them.
    ///
    /// The serialised field is a concrete FireballPoolManager only because the Inspector
    /// cannot show an interface; from OnAwake on, this class talks solely to
    /// IObjectPool&lt;ProjectileFireball&gt; (Dependency Inversion).
    /// </summary>
    public sealed class FireballWeapon : BaseWeapon
    {
        [Tooltip("The pool that hands out fireballs. Drag the FireballPool object here.")]
        [SerializeField] private FireballPoolManager fireballPool;

        [Tooltip("Where a fireball appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<ProjectileFireball> _pool;
        private Transform _firePoint;
        private IFacing _facing;

        public override WeaponType Type { get { return WeaponType.Fireball; } }

        protected override void OnAwake()
        {
            _pool = fireballPool;
            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - it never reads his scale itself.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError("[Fireball] FireballWeapon has no FireballPoolManager assigned.", this);
        }

        protected override bool FireInternal()
        {
            if (_pool == null)
                return false;

            ProjectileFireball fireball = _pool.Get();

            if (fireball == null)
            {
                // Not a warning: with a fixed-size pool this is the limit doing its job.
                Debug.Log("[Fireball] Pool exhausted - all fireballs are still in the air");
                return false;
            }

            fireball.Launch(_firePoint.position, _facing != null ? _facing.FacingDirection : 1f);
            return true;
        }
    }
}
