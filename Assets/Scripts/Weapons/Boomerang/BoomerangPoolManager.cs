using Game.Projectiles;

namespace Game.Weapons
{
    /// <summary>
    /// The boomerang's pool. Like the axe's, it exists because Unity cannot show a
    /// generic MonoBehaviour in the Inspector. The pool is sized to ONE on the scene object,
    /// and that single pooled boomerang IS the round of ammo: throwing empties the pool, so
    /// Get() returns null until the boomerang finishes its loop and comes home.
    /// </summary>
    public class BoomerangPoolManager : ProjectilePoolManager<BoomerangProjectile>
    {
    }
}
