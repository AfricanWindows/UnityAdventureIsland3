using Game.Weapons;
using UnityEngine;

/// <summary>
/// The effect the laser pickup applies: it unlocks the laser gun and nothing else.
///
/// It is a plain C# class, exactly like FireFlowerPowerUp - the object lying in the level
/// is a separate class (LaserPickable). Splitting them is what lets the same effect be
/// granted by a chest, a cheat key or an end-of-level reward without duplicating it.
///
/// Unlocking goes through the project's existing IUseableWeapon.Equip(), the very
/// mechanism the fire flower already uses. No parallel "unlock service" was added.
/// </summary>
public class LaserPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        // Searches inactive children too, so the weapon may start switched off.
        LaserWeapon laserWeapon = player.GetComponentInChildren<LaserWeapon>(true);

        if (laserWeapon == null)
        {
            Debug.LogWarning("[LaserPowerUp] No LaserWeapon under " + player.name);
            return;
        }

        laserWeapon.Equip();
        Debug.Log("[LaserPowerUp] Picked up - laser unlocked");
    }
}
