using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// Every weapon that throws something the way its owner is facing: the axe today, the
    /// fireball, the hammer from the assignment tomorrow.
    ///
    /// It exists because FireballWeapon and AxeWeapon had become identical - the same
    /// thirty-five lines twice, differing in a type name, an enum value and a log string.
    /// That is the same duplication already removed from the builders, the factories and
    /// the pool managers; it had simply grown back one layer higher.
    ///
    /// What is left in a concrete weapon is the pool reference and its WeaponType, because
    /// only those two really differ. A new thrown weapon is now about eight lines
    /// (Open/Closed).
    ///
    /// The pool is not a serialised field HERE: Unity cannot draw an Inspector slot for a
    /// generic type, so the closed subclass holds the concrete manager and hands it over
    /// through ResolvePool. From that point on this class talks only to
    /// IObjectPool&lt;T&gt; (Dependency Inversion).
    /// </summary>
    /// <typeparam name="TProjectile">What this weapon throws.</typeparam>
    public abstract class DirectionalWeapon<TProjectile> : BaseWeapon
        where TProjectile : DirectionalProjectile
    {
        [Tooltip("Where the projectile appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        private IObjectPool<TProjectile> _pool;
        private Transform _firePoint;
        private IFacing _facing;

        /// <summary>The concrete subclass hands over its serialised pool manager.</summary>
        protected abstract IObjectPool<TProjectile> ResolvePool();

        // Sealed: the setup below must happen for every thrown weapon, and a subclass that
        // overrode it and forgot to call base would be a weapon that never fires.
        protected sealed override void OnAwake()
        {
            _pool = ResolvePool();
            _firePoint = firePoint != null ? firePoint : transform;

            // Asks the owner which way he looks - it never reads his scale itself.
            _facing = GetComponentInParent<IFacing>();

            if (_pool == null)
                Debug.LogError(LogPrefix + " has no pool manager assigned.", this);

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
