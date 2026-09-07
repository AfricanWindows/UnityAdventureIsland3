using UnityEngine;

/// <summary>
/// "This counts as fruit eaten."
///
/// Deliberately separate from AddPowerPowerUp: how much power a fruit restores and whether
/// it counts towards the twenty are two different rules, and the assignment sets them
/// independently - a banana gives 1 power, a carrot gives 2, but both count as ONE fruit.
/// </summary>
public class CountFruitPowerUp : IPowerUp
{
    private readonly int amount;

    public CountFruitPowerUp(int amount)
    {
        this.amount = amount;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        FruitCounterController counter = player.GetComponentInChildren<FruitCounterController>(true);

        if (counter == null)
        {
            Debug.LogWarning("[Fruit] No FruitCounterController under " + player.name);
            return;
        }

        counter.Collect(amount);
    }
}
