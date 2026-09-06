using UnityEngine;

/// <summary>
/// "Eating this refills the power bar by N segments."
///
/// One class for every fruit in the game: a banana is this with 1, a carrot is this with
/// 2, and a fruit worth five is a prefab, not a new script (Open/Closed).
///
/// It is a plain C# class, like every other IPowerUp here - the thing lying in the level
/// is a separate class (FruitPickable). Splitting them is what lets the same effect be
/// granted by a chest, an egg or an end-of-level reward without duplicating it.
/// </summary>
public class AddPowerPowerUp : IPowerUp
{
    private readonly int amount;

    public AddPowerPowerUp(int amount)
    {
        this.amount = amount;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        PowerController power = player.GetComponentInChildren<PowerController>(true);

        if (power == null)
        {
            Debug.LogWarning("[Fruit] No PowerController under " + player.name);
            return;
        }

        int added = power.AddPower(amount);

        // Says how much actually fitted, which is not always what was asked: a carrot at
        // 14 of 15 segments adds one.
        Debug.Log("[Fruit] +" + added + " power");
    }
}
