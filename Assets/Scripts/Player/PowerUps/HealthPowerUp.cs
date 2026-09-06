using UnityEngine;

/// <summary>Exercise item 2: the heart gives Mario one health point, up to the maximum.</summary>
public class HealthPowerUp : IPowerUp
{
    private readonly int amount;

    public HealthPowerUp(int amount)
    {
        this.amount = amount;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        PlayerHealthController health = player.GetComponent<PlayerHealthController>();
        if (health == null)
        {
            Debug.LogWarning("HealthPowerUp: no PlayerHealthController on " + player.name);
            return;
        }

        health.AddHealth(amount);
    }
}
