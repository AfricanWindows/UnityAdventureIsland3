using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The boomerang launcher - built exactly like LaserWeapon: a BaseWeapon subclass that
    /// only decides WHEN a boomerang may leave the hand and where it starts. It does not
    /// build, move or destroy boomerangs; it borrows one and the boomerang brings itself
    /// back.
    ///
    /// "One round, no re-throw until it is home" is enforced two ways that agree: the pool
    /// holds exactly one instance (empty pool = no throw), and CanFire also refuses while
    /// the borrowed boomerang still reports IsFlying.
    ///
    /// The serialized field is a concrete BoomerangPoolManager only because the Inspector
    /// cannot show an interface; from OnAwake on, this class talks solely to
    /// IObjectPool&lt;BoomerangProjectile&gt;. WeaponsHandler picks it up through IWeapon, so
    /// adding it to Mario needs no handler edit.
    /// </summary>
    public sealed class BoomerangWeapon : BaseWeapon
    {
        [Tooltip("The pool that hands out the boomerang. Drag the BoomerangPool object here.")]
        [SerializeField] private BoomerangPoolManager boomerangPool;

        [Tooltip("Where a boomerang appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<BoomerangProjectile> _pool;
        private Transform _firePoint;
        private IFacing _facing;
        private BoomerangProjectile _inFlight;

        public override WeaponType Type { get { return WeaponType.Boomerang; } }

        /// <summary>Unlocked, off cooldown, AND the one boomerang is already home.</summary>
        public override bool CanFire
        {
            get { return base.CanFire && !IsBoomerangOut; }
        }

        // Unity's overloaded == makes a destroyed boomerang read as null, so this turns
        // false the moment the object is gone - the weapon never jams on a corpse.
        private bool IsBoomerangOut
        {
            get { return _inFlight != null && _inFlight.IsFlying; }
        }

        protected override void OnAwake()
        {
            _pool = boomerangPool;
            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - never reads his scale directly.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError("[Boomerang] BoomerangWeapon has no BoomerangPoolManager assigned.", this);
        }

        protected override bool FireInternal()
        {
            if (_pool == null || IsBoomerangOut)
                return false;

            BoomerangProjectile boomerang = _pool.Get();

            if (boomerang == null)
            {
                // Not a warning: with a pool of one this is the rule doing its job.
                Debug.Log("[Boomerang] Still in the air - wait for it to come back");
                return false;
            }

            _inFlight = boomerang;
            float facing = _facing != null ? _facing.FacingDirection : 1f;
            // The fire point rides on Mario, so the boomerang's return leg follows him.
            boomerang.Throw(_firePoint.position, _firePoint, facing);
            return true;
        }
    }
}
