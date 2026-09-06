/// <summary>
/// The boomerang pickup sitting in the level - the exact counterpart of LaserPickable.
/// Everything about being picked up - waiting for the player's trigger, checking the tag,
/// handing the effect over, disappearing - is already written once in BasePickable. This
/// class only answers WHAT Mario gets.
/// </summary>
public class BoomerangPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new BoomerangPowerUp();
    }
}
