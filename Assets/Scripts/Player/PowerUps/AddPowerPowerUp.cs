using UnityEngine;

/// <summary>
/// ROLE: Effect: refill the power bar by N segments.
/// PATTERNS: Template Method - fills Apply of PlayerComponentPowerUp; Composite - a leaf in the fruit.
///
/// "Eating this refills the power bar by N segments."
///
/// One class for every fruit in the game: a banana is this with 1, a carrot is this with
/// 2, and a fruit worth five is a prefab, not a new script (Open/Closed).
///
/// It is a plain C# class, like every other IPowerUp here - the thing lying in the level
/// is a separate class (FruitPickable). Splitting them is what lets the same effect be
/// granted by a chest, an egg or an end-of-level reward without duplicating it. Finding the
/// power bar on the player is PlayerComponentPowerUp's job; this class only adds.
/// </summary>
public class AddPowerPowerUp : PlayerComponentPowerUp<IPowerWallet>
{
    private readonly int amount;

    public AddPowerPowerUp(int amount)
    {
        this.amount = amount;
    }

    protected override void Apply(IPowerWallet power, GameObject player)
    {
        // The bar clamps it: a carrot at 14 of 15 segments adds one.
        power.AddPower(amount);
    }
}
