using UnityEngine;

public class StarPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        PlayerInvincible invincible = player.GetComponent<PlayerInvincible>();
        if (invincible == null)
        {
            Debug.LogWarning("StarPowerUp: no PlayerInvincible on " + player.name);
            return;
        }

        invincible.ActivateInvincibility();
    }
}
