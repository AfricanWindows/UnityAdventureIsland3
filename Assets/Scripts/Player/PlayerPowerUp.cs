using UnityEngine;

/// <summary>
/// ROLE: The player's hand for power-ups: applies an IPowerUp to him.
/// PATTERNS: none - a plain component.
/// SOLID: D - pickups know it only as IPowerUpCollector.
///
/// The player's hand for power-ups: a pickable gives it an IPowerUp, and this applies it to
/// the player. Pickables know it only as IPowerUpCollector (Dependency Inversion).
/// </summary>
public class PlayerPowerUp : MonoBehaviour, IPowerUpCollector
{
    public void CollectPowerUp(IPowerUp powerUp)
    {
        powerUp.ApplyPowerUp(this.gameObject);
    }
}
