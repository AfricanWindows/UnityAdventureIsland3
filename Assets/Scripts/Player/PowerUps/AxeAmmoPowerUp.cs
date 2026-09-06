using UnityEngine;

public class AxeAmmoPowerUp : IPowerUp
{
    private readonly int amount;

    public AxeAmmoPowerUp(int amount)
    {
        this.amount = amount;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        AxeWeapon axeWeapon = player.GetComponentInChildren<AxeWeapon>();
        if (axeWeapon == null)
        {
            Debug.LogWarning("AxeAmmoPowerUp: no AxeWeapon under " + player.name);
            return;
        }

        axeWeapon.AddAmmo(amount);
    }
}
