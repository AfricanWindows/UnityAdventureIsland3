using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// ROLE: The boomerang launcher.
    /// PATTERNS: Template Method - fills Launch and extends CanFire; Pooling; DI - the pool in Inject.
    ///
    /// The boomerang launcher. It only decides WHEN a boomerang may leave the hand; it does
    /// not build, move or destroy boomerangs - it borrows one, and the boomerang brings
    /// itself back.
    ///
    /// "One round, no re-throw until it is home" is enforced two ways that agree: the pool
    /// holds exactly one instance (empty pool = no throw), and CanFire also refuses while
    /// the borrowed boomerang still reports IsFlying.
    ///
    /// The pool, the fire point and the facing are ProjectileWeapon's, shared with the axe.
    /// What is the boomerang's own is the launch: it is thrown with the fire point as its
    /// OWNER, because the fire point rides on the player and the return leg must follow him.
    /// That is why it is not a DirectionalWeapon - a straight shot forgets its owner.
    /// </summary>
    public sealed class BoomerangWeapon : ProjectileWeapon<BoomerangProjectile>
    {
        private BoomerangProjectile _inFlight;

        /// <summary>Unlocked, off cooldown, AND the one boomerang is already home.</summary>
        protected override bool CanFire
        {
            get { return base.CanFire && !IsBoomerangOut; }
        }

        // Unity's overloaded == makes a destroyed boomerang read as null, so this turns
        // false the moment the object is gone - the weapon never jams on a corpse.
        private bool IsBoomerangOut
        {
            get { return _inFlight != null && _inFlight.IsFlying; }
        }

        protected override void Launch(BoomerangProjectile boomerang, Transform from, float facing)
        {
            _inFlight = boomerang;
            boomerang.Throw(from.position, from, facing);
        }
    }
}
