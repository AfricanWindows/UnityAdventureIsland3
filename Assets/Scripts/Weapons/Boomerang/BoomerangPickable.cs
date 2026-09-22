using Game.Weapons;

/// <summary>
/// The boomerang pickup sitting in the level.
/// Being picked up is written once in BasePickable; handing over a weapon is written once in
/// WeaponPickable and EquipWeaponPowerUp. This class only names which weapon.
/// </summary>
public class BoomerangPickable : WeaponPickable<BoomerangWeapon>
{
}
