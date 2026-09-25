using UnityEngine;

/// <summary>
/// ROLE: Effect: count one fruit.
/// PATTERNS: Template Method - fills Apply; Composite - a leaf in the fruit.
///
/// "This counts as fruit eaten."
///
/// Deliberately separate from AddPowerPowerUp: how much power a fruit restores and whether
/// it counts towards the twenty are two different rules, and the assignment sets them
/// independently - a banana gives 1 power, a carrot gives 2, but both count as ONE fruit.
/// Finding the counter on the player is PlayerComponentPowerUp's job; this class only counts.
/// </summary>
public class CountFruitPowerUp : PlayerComponentPowerUp<IFruitCollector>
{
    private readonly int amount;

    public CountFruitPowerUp(int amount)
    {
        this.amount = amount;
    }

    protected override void Apply(IFruitCollector counter, GameObject player)
    {
        counter.Collect(amount);
    }
}
