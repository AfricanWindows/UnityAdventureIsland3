using UnityEngine;

/// <summary>
/// A fruit lying in the level. Banana, carrot, melon - all the same class, told apart by
/// one number in the Inspector and a different sprite (Open/Closed: a new fruit is a new
/// prefab, never a new script).
///
/// Everything about being picked up - waiting for the player's trigger, checking the tag,
/// handing the effect over, DISAPPEARING - is already written once in BasePickable, this
/// project's template method for pickups. So this class only answers what the fruit is
/// worth.
/// </summary>
public class FruitPickable : BasePickable
{
    [Tooltip("Segments this fruit restores. Banana = 1, carrot = 2.")]
    [SerializeField] private int powerAmount = 1;

    protected override IPowerUp CreatePowerUp()
    {
        return new AddPowerPowerUp(powerAmount);
    }
}
