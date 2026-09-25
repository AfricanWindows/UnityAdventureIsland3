/// <summary>
/// ROLE: "Am I protected right now?" - the fairy, the recovery window, the death animation.
/// PATTERNS: none - a role interface.
/// SOLID: D - PlayerDeath never names a power-up.
///
/// Anything that can make the player temporarily immune to death.
/// PlayerDeath depends on this abstraction, never on a concrete power-up class.
/// </summary>
public interface IInvincible
{
    bool IsInvincible { get; }
}
