using Game.Weapons;
using UnityEngine;

/// <summary>
/// The effect the boomerang pickup applies: it unlocks the boomerang launcher and nothing
/// else. A plain C# class, exactly like LaserPowerUp - the item lying in the level is a
/// separate class (BoomerangPickable).
///
/// Unlocking goes through the project's existing IUseableWeapon.Equip(), the same
/// mechanism the fire flower and the laser already use.
/// </summary>
public class BoomerangPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        // Searches inactive children too, so the weapon may start switched off.
        BoomerangWeapon boomerangWeapon = player.GetComponentInChildren<BoomerangWeapon>(true);

        if (boomerangWeapon == null)
        {
            Debug.LogWarning("[BoomerangPowerUp] No BoomerangWeapon under " + player.name);
            return;
        }

        boomerangWeapon.Equip();
        Debug.Log("[BoomerangPowerUp] Picked up - boomerang unlocked");
    }
}
