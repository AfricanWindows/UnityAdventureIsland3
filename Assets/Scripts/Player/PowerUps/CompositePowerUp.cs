using UnityEngine;

/// <summary>
/// COMPOSITE. One pickup, several effects - and every effect stays a small class that does
/// exactly one thing.
///
/// A fruit both refills the power bar AND counts towards the twenty that cost a life. The
/// alternative was to teach AddPowerPowerUp to also count fruit, which would have given it
/// two reasons to change and made it useless for anything that grants power without being
/// a fruit (Single Responsibility).
///
/// It is an IPowerUp made of IPowerUps, so it can hold a composite of its own. The egg
/// from the assignment - "an animal, a weapon or a fairy appears" - is the next thing that
/// will want this.
/// </summary>
public class CompositePowerUp : IPowerUp
{
    private readonly IPowerUp[] effects;

    public CompositePowerUp(params IPowerUp[] effects)
    {
        this.effects = effects;
    }

    public void ApplyPowerUp(GameObject player)
    {
        if (effects == null)
            return;

        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null)
                effects[i].ApplyPowerUp(player);
        }
    }
}
