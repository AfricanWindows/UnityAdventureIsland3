using Game.Projectiles;

namespace Game.Weapons
{
    /// <summary>
    /// The laser's pool, and deliberately EMPTY.
    ///
    /// Every line it used to contain - building the chain, creating the root container,
    /// forwarding Get and Release - now lives once in ProjectilePoolManager&lt;T&gt;.
    ///
    /// Why the empty subclass has to exist at all: Unity does not serialise the fields of
    /// a generic MonoBehaviour, so a component in the Inspector must be a closed,
    /// non-generic type. A one-line heir gives the engine that type while the logic stays
    /// in one place. This is an engine limitation, not a design decision - and it is the
    /// only duplication in the weapon layer that could not be removed.
    /// </summary>
    public class LaserPoolManager : ProjectilePoolManager<LaserProjectile>
    {
    }
}
