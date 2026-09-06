using Game.Weapons;

/// <summary>The fire flower: unlocks the fireball.</summary>
public class FireFlowerController : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new EquipWeaponPowerUp<FireballWeapon>();
    }
}
