using Game.Core;
using Game.Core.DI;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The boomerang launcher - a BaseWeapon subclass that only decides WHEN a boomerang
    /// may leave the hand and where it starts. It does not build, move or destroy
    /// boomerangs; it borrows one and the boomerang brings itself back.
    ///
    /// "One round, no re-throw until it is home" is enforced two ways that agree: the pool
    /// holds exactly one instance (empty pool = no throw), and CanFire also refuses while
    /// the borrowed boomerang still reports IsFlying.
    ///
    /// Like the axe, it does not go looking for its pool. GameInstaller registers the pool
    /// once as IObjectPool&lt;BoomerangProjectile&gt; and hands it over before Awake, so the
    /// pool object can live anywhere in the scene and the player prefab needs no reference
    /// to it - a prefab cannot hold one anyway (Dependency Inversion).
    ///
    /// It does not inherit DirectionalWeapon: a boomerang does not fly straight ahead and
    /// forget about its owner, it comes back to him. Sharing the base class would mean
    /// weakening it for the one weapon that does not fit.
    /// </summary>
    public sealed class BoomerangWeapon : BaseWeapon, IInjectable
    {
        [Tooltip("Optional override. Normally EMPTY: the pool arrives through injection, " +
                 "so the pool object can live anywhere in the scene.")]
        [SerializeField] private BoomerangPoolManager boomerangPool;

        [Tooltip("Where a boomerang appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<BoomerangProjectile> _pool;
        private IObjectPool<BoomerangProjectile> _injectedPool;
        private Transform _firePoint;
        private IFacing _facing;
        private BoomerangProjectile _inFlight;

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

        /// <summary>Called by GameInstaller before Awake. Resolved once, cached, done.</summary>
        public void Inject(IServiceContainer container)
        {
            if (container != null)
                container.TryResolve(out _injectedPool);
        }

        protected override void OnAwake()
        {
            // Compared while the field still has its concrete Unity type, so a manager
            // deleted from the scene reads as a real null and the injected pool takes over.
            _pool = boomerangPool != null ? boomerangPool : _injectedPool;

            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - never reads his scale directly.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError("[Boomerang] has no pool. Put a BoomerangProjectile pool " +
                               "object in the scene, or assign one on this weapon.", this);
        }

        protected override bool FireInternal()
        {
            if (_pool == null || IsBoomerangOut)
                return false;

            BoomerangProjectile boomerang = _pool.Get();

            // Empty pool = the one boomerang is still in the air: the rule doing its job.
            if (boomerang == null)
                return false;

            _inFlight = boomerang;
            float facing = _facing != null ? _facing.FacingDirection : 1f;
            // The fire point rides on the player, so the return leg follows him.
            boomerang.Throw(_firePoint.position, _firePoint, facing);
            return true;
        }
    }
}
