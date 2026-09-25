/// <summary>
/// ROLE: Anything a projectile can hurt (TakeDamage).
/// PATTERNS: none - a role interface.
/// SOLID: D - a projectile never knows what it hit.
///
/// Anything a projectile can hurt. Projectiles talk to this interface,
/// so they never need to know what kind of enemy they hit.
///
/// It answers whether the blow LANDED, because the one who struck needs to know: a piercing
/// boomerang flies on through what it hurt, but not through what it bounced off - the ghost
/// that no weapon can scratch, the stone that shrugs off an axe. The target is the only one
/// who can tell the two apart, so it is the target that says (Tell, Don't Ask).
/// </summary>
public interface IDamageable
{
    /// <returns>True if the blow did harm, whether or not the target died of it. False if it
    /// bounced off: armour, a blow too light to matter, a target that is already down.</returns>
    bool TakeDamage(int amount);
}
