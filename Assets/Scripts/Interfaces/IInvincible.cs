/// <summary>
/// Anything that can make the player temporarily immune to death.
/// PlayerDeath depends on this abstraction, never on a concrete power-up class.
/// </summary>
public interface IInvincible
{
    bool IsInvincible { get; }
}
