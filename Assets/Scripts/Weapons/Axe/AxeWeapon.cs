using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The axe thrower. It decides WHEN an axe may leave Mario's hand and where it starts -
    /// nothing else. It does not build axes, does not own them, and does not destroy them;
    /// it borrows one and the axe brings itself back (Single Responsibility).
    ///
    /// The axe is UNLIMITED once found, exactly as in Adventure Island: there is no ammo
    /// count, no reloading and no counter. Two things replaced all of that, and neither of
    /// them lives in this class:
    ///   - AxePickable unlocks the weapon through the shared EquipWeaponPowerUp
    ///   - WeaponsLostOnDeath takes it away again when Mario dies
    /// So "how you get it" and "how you lose it" are rules of the game, not of the axe,
    /// and they apply to every weapon without this file knowing about them (Open/Closed).
    ///
    /// What DOES limit rapid fire is the pool: five axes exist, and a sixth throw has to
    /// wait until one of them lands. That is a deliberate cap on things in flight, not an
    /// ammo count - it refills by itself.
    /// </summary>
    public sealed class AxeWeapon : BaseWeapon
    {
        [Tooltip("The pool that hands out axes. Drag the AxePool object here.")]
        [SerializeField] private AxePoolManager axePool;

        [Tooltip("Where an axe appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<ProjectileAxe> _pool;
        private Transform _firePoint;
        private IFacing _facing;

        public override WeaponType Type { get { return WeaponType.Axe; } }

        protected override void OnAwake()
        {
            _pool = axePool;
            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - it never reads his scale itself.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError("[Axe] AxeWeapon has no AxePoolManager assigned.", this);
        }

        protected override bool FireInternal()
        {
            if (_pool == null)
                return false;

            ProjectileAxe axe = _pool.Get();

            if (axe == null)
            {
                // Not a warning: with a fixed-size pool this is the "five in the air at
                // once" rule doing its job. Nothing is fired until one comes back.
                Debug.Log("[Axe] All five axes are still in the air");
                return false;
            }

            axe.Launch(_firePoint.position, _facing != null ? _facing.FacingDirection : 1f);
            return true;
        }
    }
}
