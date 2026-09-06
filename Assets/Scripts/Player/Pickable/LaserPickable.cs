using UnityEngine;

/// <summary>
/// The laser pickup sitting in the level. Exercise 3's power-up item.
///
/// Everything about being picked up - waiting for the player's trigger, checking the tag,
/// handing the effect over, disappearing - is already written once in BasePickable, which
/// is this project's template method for pickups. So this class is three lines: it only
/// answers WHAT Mario gets.
/// </summary>
public class LaserPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new LaserPowerUp();
    }
}
