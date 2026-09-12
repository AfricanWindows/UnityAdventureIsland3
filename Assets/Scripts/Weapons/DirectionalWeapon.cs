using Game.Core;
using Game.Core.DI;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// Every weapon that throws something the way its owner is facing: the axe today, the
    /// hammer from the assignment tomorrow.
    ///
    /// It exists because two thrown weapons had become identical - the same thirty-five
    /// lines twice, differing in a type name, an enum value and a log string. That is the
    /// same duplication already removed from the builders, the factories and the pool
    /// managers; it had simply grown back one layer higher.
    ///
    /// WHERE THE POOL COMES FROM is the interesting part. A pool is a thing that lives in
    /// the SCENE - one object, shared, parked at the root so it never moves. A weapon
    /// lives on the PLAYER PREFAB. A prefab cannot hold a reference to a scene object, so
    /// wiring the two by hand means an override on the player's scene instance, which
    /// breaks the moment the prefab is re-used.
    ///
    /// So the pool ARRIVES instead: GameInstaller registers it once as IObjectPool&lt;T&gt;
    /// and hands it over before Awake. The weapon never searches for it, never names the
    /// manager that owns it, and does not care whether the pool sits on its own object, in
    /// a level, or somewhere else entirely (Dependency Inversion). This is exactly how
    /// ShooterEnemy has always got its shots - one mechanism for the whole game, rather
    /// than one for enemies and another for the player.
    ///
    /// The Inspector field is kept as an OVERRIDE for the case where a weapon must use one
    /// specific pool. Empty is the normal state.
    /// </summary>
    /// <typeparam name="TProjectile">What this weapon throws.</typeparam>
    public abstract class DirectionalWeapon<TProjectile> : BaseWeapon, IInjectable
        where TProjectile : DirectionalProjectile
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
        /// Called by GameInstaller before Awake. The weapon asks for the one thing it
        /// needs and keeps neither the container nor any knowledge of who registered it.
        /// </summary>
        public void Inject(IServiceContainer container)
        {
            if (container != null)
                container.TryResolve(out _injectedPool);
        }

        // Sealed: the setup below must happen for every thrown weapon, and a subclass that
        // overrode it and forgot to call base would be a weapon that never fires.
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

            OnWeaponReady();
        }

        /// <summary>Subclass setup, if it needs any. Most do not.</summary>
        protected virtual void OnWeaponReady() { }

        protected sealed override bool FireInternal()
        {
            if (_pool == null)
                return false;

            TProjectile projectile = _pool.Get();

            if (projectile == null)
            {
                // Not a warning: with a fixed-size pool this is the ammo limit doing its
                // job. Nothing is fired until one of the projectiles in the air comes back.
                Debug.Log(LogPrefix + " all projectiles are still in the air");
                return false;
            }

            projectile.Launch(_firePoint.position, _facing != null ? _facing.FacingDirection : 1f);
            return true;
        }
    }
}
