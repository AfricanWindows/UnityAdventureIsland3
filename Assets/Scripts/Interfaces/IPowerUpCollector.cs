/// <summary>
/// ROLE: "I can receive a power-up" - the player's hand.
/// PATTERNS: none - a role interface.
/// SOLID: D - pickups never name PlayerPowerUp.
///
/// "I can receive a power-up." What a pickable hands its effect to.
/// Pickables depend on this, never on the concrete PlayerPowerUp (Dependency Inversion).
/// </summary>
public interface IPowerUpCollector
{
    void CollectPowerUp(IPowerUp powerUp);
}
