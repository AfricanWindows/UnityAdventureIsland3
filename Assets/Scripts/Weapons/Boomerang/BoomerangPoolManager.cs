using Game.Projectiles;

namespace Game.Weapons
{
    /// <summary>
    /// ROLE: The boomerang pool - one boomerang, no new throw until it is home.
    /// PATTERNS: Pooling; Generics - an empty closed type of ProjectilePoolManager.
    ///
    /// The boomerang's pool. Like the axe's, it exists because Unity cannot show a
    /// generic MonoBehaviour in the Inspector. The pool is sized to ONE on the scene object,
    /// and that single pooled boomerang IS the round of ammo: throwing empties the pool, so
    /// Get() returns null until the boomerang finishes its loop and comes home.
    /// </summary>
    public class BoomerangPoolManager : ProjectilePoolManager<BoomerangProjectile>
    {
    }
}
