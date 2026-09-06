using Game.Weapons;

/// <summary>
/// The laser pickup sitting in the level.
///
/// Everything about being picked up - waiting for the player's trigger, checking the tag,
/// handing the effect over, disappearing - is already written once in BasePickable, which
/// is this project's template method for pickups. So this class only answers WHAT Mario
/// gets, and even that is now one generic type argument.
/// </summary>
public class LaserPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new EquipWeaponPowerUp<LaserWeapon>();
    }
}
