using Game.Weapons;

/// <summary>
/// The axe pickup lying in the level.
///
/// It grants the weapon itself, not ammunition: in Adventure Island the axe is unlimited
/// once found, and it is lost by dying rather than by being spent. Counting rounds moved
/// out of the game entirely - see WeaponsLostOnDeath for the other half of that rule.
/// </summary>
public class AxePickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new EquipWeaponPowerUp<AxeWeapon>();
    }
}
