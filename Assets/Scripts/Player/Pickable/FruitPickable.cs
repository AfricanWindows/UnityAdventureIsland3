using UnityEngine;

/// <summary>
/// A fruit lying in the level. Banana, carrot, melon - all the same class, told apart by
/// the numbers in the Inspector and a different sprite (Open/Closed: a new fruit is a new
/// prefab, never a new script).
///
/// Everything about being picked up - waiting for the player's trigger, checking the tag,
/// handing the effect over, DISAPPEARING - is already written once in BasePickable, this
/// project's template method for pickups.
///
/// Eating one does two independent things, so it hands over a composite of two small
/// effects rather than one class that knows about both.
/// </summary>
public class FruitPickable : BasePickable
{
    [Tooltip("Power segments this fruit restores. Banana = 1, carrot = 2.")]
    [SerializeField] private int powerAmount = 1;

    [Tooltip("How many fruit this counts as. Normally 1 - a carrot is worth more power, " +
             "but it is still one fruit out of the twenty that cost a life.")]
    [SerializeField] private int fruitCount = 1;

    protected override IPowerUp CreatePowerUp()
    {
        return new CompositePowerUp(
            new AddPowerPowerUp(powerAmount),
            new CountFruitPowerUp(fruitCount));
    }
}
