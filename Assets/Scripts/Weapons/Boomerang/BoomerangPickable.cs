using Game.Weapons;

/// <summary>
/// The boomerang pickup sitting in the level - the exact counterpart of LaserPickable.
/// Being picked up is written once in BasePickable; unlocking a weapon is written once in
/// EquipWeaponPowerUp. This class only names which weapon.
/// </summary>
public class BoomerangPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new EquipWeaponPowerUp<BoomerangWeapon>();
    }
}
