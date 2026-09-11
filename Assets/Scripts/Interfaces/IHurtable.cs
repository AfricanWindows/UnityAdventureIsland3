using UnityEngine;

/// <summary>
/// "Something hurt me, but did not kill me."
///
/// Deliberately separate from IKillable. A campfire, a ghost and an enemy touch KILL - one
/// call, no arguments, no answer needed. A stone costs power and throws you back, and the
/// hazard has to know whether the hit actually landed, because a player who is still in his
/// invincibility window must not be hit again by the same stone (Interface Segregation).
///
/// A hazard therefore says WHAT it costs and WHICH WAY it throws, and knows nothing about
/// power bars, invincibility windows, Rigidbodies or respawning - all of that lives on the
/// player, behind this one method (Dependency Inversion).
/// </summary>
public interface IHurtable
{
    /// <summary>
    /// Take a non-lethal hit.
    /// </summary>
    /// <param name="powerCost">Segments of the power bar to remove.</param>
    /// <param name="knockback">Velocity to be thrown away with, in world units.</param>
    /// <returns>False if the hit was ignored - already invincible, or already recovering.</returns>
    bool TryHurt(int powerCost, Vector2 knockback);
}
