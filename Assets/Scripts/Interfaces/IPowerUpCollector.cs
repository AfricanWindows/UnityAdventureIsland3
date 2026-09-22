/// <summary>
/// "I can receive a power-up." What a pickable hands its effect to.
/// Pickables depend on this, never on the concrete PlayerPowerUp (Dependency Inversion).
/// </summary>
public interface IPowerUpCollector
{
    void CollectPowerUp(IPowerUp powerUp);
}
