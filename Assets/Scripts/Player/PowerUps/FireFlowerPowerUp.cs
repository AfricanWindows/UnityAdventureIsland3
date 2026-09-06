using UnityEngine;

public class FireFlowerPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if(player != null)
        {
            Debug.Log("FireFlowerPowerUp applied to " + player.name);
            FireballWeapon fireballWeapon = player.GetComponentInChildren<FireballWeapon>();
            if(fireballWeapon != null)
            {
                fireballWeapon.Equip();
            }
        }
    }
}
