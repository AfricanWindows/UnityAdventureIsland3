using UnityEngine;

public class KeyPowerUp : IPowerUp
{
    private readonly int amount;

    public KeyPowerUp(int amount)
    {
        this.amount = amount;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
        if (playerKeys == null)
        {
            Debug.LogWarning("KeyPowerUp: no PlayerKeys on " + player.name);
            return;
        }

        playerKeys.AddKey(amount);
    }
}
