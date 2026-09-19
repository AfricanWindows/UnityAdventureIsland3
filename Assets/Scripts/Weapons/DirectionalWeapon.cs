using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// Every weapon that throws something straight the way its owner is facing and forgets
    /// about it: the axe, and the red and blue animals' shots.
    ///
    /// Everything about getting a projectile - the pool, the fire point, the facing - is
    /// ProjectileWeapon's. This class only fills in how a straight shot leaves, and leaves
    /// one gap of its own: OnBeforeLaunch, where a weapon may dress the projectile first.
    /// </summary>
    /// <typeparam name="TProjectile">What this weapon throws.</typeparam>
    public abstract class DirectionalWeapon<TProjectile> : ProjectileWeapon<TProjectile>
        where TProjectile : DirectionalProjectile
    {
        protected sealed override void Launch(TProjectile projectile, Transform from, float facing)
        {
            OnBeforeLaunch(projectile);
            projectile.Launch(from.position, facing);
        }

        /// <summary>
        /// Last chance to change the projectile that is about to fly. Empty for the axe,
        /// which looks the same whoever throws it; the animals use it to give the shot their
        /// own sprite, which is what lets two animals share one pool.
        /// </summary>
        protected virtual void OnBeforeLaunch(TProjectile projectile) { }
    }
}
