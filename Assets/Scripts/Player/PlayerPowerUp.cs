using UnityEngine;

/// <summary>
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
