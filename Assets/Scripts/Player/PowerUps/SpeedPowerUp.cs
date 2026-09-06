using UnityEngine;

/// <summary>Exercise item 1: what the lightning gives Mario.</summary>
public class SpeedPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        PlayerSpeedBoost speedBoost = player.GetComponent<PlayerSpeedBoost>();
        if (speedBoost == null)
        {
            Debug.LogWarning("SpeedPowerUp: no PlayerSpeedBoost on " + player.name);
            return;
        }

        speedBoost.ActivateSpeedBoost();
    }
}
