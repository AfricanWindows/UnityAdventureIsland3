using UnityEngine;

/// <summary>
/// ROLE: Several effects handed over as one (a fruit = power + one fruit counted).
/// PATTERNS: Composite - the composite itself.
/// SOLID: S - every effect stays a small class that does one thing.
///
/// COMPOSITE. One pickup, several effects - and every effect stays a small class that does
/// exactly one thing.
///
/// A fruit both refills the power bar AND counts towards the twenty that earn a life. The
/// alternative was to teach AddPowerPowerUp to also count fruit, which would have given it
/// two reasons to change and made it useless for anything that grants power without being
/// a fruit (Single Responsibility).
///
/// It is an IPowerUp made of IPowerUps, so a pickable still hands over ONE effect whatever
/// is inside it: BasePickable never learns that a fruit does two things.
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
