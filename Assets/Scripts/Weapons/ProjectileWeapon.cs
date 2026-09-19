using Game.Core;
using Game.Core.DI;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// Every weapon that borrows its projectile from a pool: the axe, the animals' shots and
    /// the boomerang. What they share is written here once - where the pool comes from,
    /// where the projectile appears, which way the owner is looking, and the order of a
    /// shot. The ONE step that differs - how the projectile is sent off - is left abstract
    /// (Template Method): a straight throw for DirectionalWeapon, an out-and-back loop for
    /// the boomerang.
    ///
    /// The boomerang used to carry its own copy of all this, because it does not fit
    /// DirectionalWeapon ("fly the way I face and forget me"). The fix was not to bend
    /// DirectionalWeapon but to put the shared part one level HIGHER, where both fit.
    ///
    /// WHERE THE POOL COMES FROM. A pool lives in the SCENE, a weapon on the PLAYER PREFAB,
    /// and a prefab cannot hold a reference to a scene object. So the pool ARRIVES:
    /// GameInstaller registers it once as IObjectPool&lt;T&gt; and hands it over before Awake
    /// (Dependency Inversion). The Inspector field on the concrete weapon is only an
    /// override; empty is the normal state.
    /// </summary>
    /// <typeparam name="TProjectile">What this weapon fires.</typeparam>
    public abstract class ProjectileWeapon<TProjectile> : BaseWeapon, IInjectable
        where TProjectile : BaseProjectile
    {
        [Tooltip("Where the projectile appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<TProjectile> _pool;
        private IObjectPool<TProjectile> _injectedPool;
        private Transform _firePoint;
        private IFacing _facing;

        /// <summary>
        /// The pool assigned by hand in the Inspector, or null when there is none - which
        /// is the normal case. Unity cannot draw a slot for a generic type, so only the
        /// closed subclass can hold that field.
        /// </summary>
        protected abstract IObjectPool<TProjectile> ResolveInspectorPool();

        /// <summary>
        /// The one step each weapon writes itself: send this projectile off from this point,
        /// for an owner looking this way (-1 left, 1 right).
        /// </summary>
        protected abstract void Launch(TProjectile projectile, Transform from, float facing);

        /// <summary>Called by GameInstaller before Awake.</summary>
        public void Inject(IServiceContainer container)
        {
            if (container != null)
                container.TryResolve(out _injectedPool);
        }

        // Sealed: this setup must happen for every weapon, and a subclass that overrode it
        // and forgot to call base would be a weapon that never fires.
        protected sealed override void OnAwake()
        {
            // The hand-assigned pool wins, so a weapon CAN be pinned to a specific one.
            IObjectPool<TProjectile> assigned = ResolveInspectorPool();
            _pool = assigned != null ? assigned : _injectedPool;

            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - it never reads his scale itself.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError(LogPrefix + " has no pool. Put a " + typeof(TProjectile).Name +
                               " pool object in the scene, or assign one on this weapon.", this);
        }

        // The fixed order of a shot - take from the pool, check it, launch it. Sealed, so no
        // weapon can rearrange it; only Launch() is open.
        protected sealed override bool FireInternal()
        {
            if (_pool == null)
                return false;

            TProjectile projectile = _pool.Get();

            // Empty pool = the ammo limit doing its job: nothing is fired until one of the
            // projectiles in the air comes back. For the boomerang the limit is one.
            if (projectile == null)
                return false;

            Launch(projectile, _firePoint, _facing != null ? _facing.FacingDirection : 1f);
            return true;
        }
    }
}
