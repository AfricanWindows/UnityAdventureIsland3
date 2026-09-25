using UnityEngine;

/// <summary>
/// ROLE: Helper: "can anything on the player take this hit instead of him?" (the animal).
/// PATTERNS: none - an extension method.
/// SOLID: O - a new IHitAbsorber (a shield) is covered without editing any hazard.
///
/// "Ask whether anything on the player can take this hit for him."
///
/// Written here once, as an extension, because the question is asked from three unrelated
/// places - the stone, everything that kills on touch, and the enemies' shots - and none of
/// them should carry a copy of the loop (Don't Repeat Yourself). An extension method rather
/// than a component: there is no state to keep, and the call reads like a sentence at the
/// place it matters.
///
/// It says nothing about animals. Whatever answers IHitAbsorber gets asked, so a shield or
/// a bodyguard added later is covered without touching a single hazard (Open/Closed).
/// </summary>
public static class HitAbsorberExtensions
{
    /// <summary>
    /// True when something took the hit and the player must be left alone. False means
    /// carry on and hurt him.
    /// </summary>
    /// <param name="player">The object about to be hurt.</param>
    /// <param name="source">What is hurting him. Handed to the absorber, which may want to
    /// destroy it - a stone smashed by the animal the player is riding.</param>
    public static bool TryAbsorbHit(this GameObject player, GameObject source)
    {
        if (player == null)
            return false;

        // Absorbers live ON the player, next to his health and his invincibility, so a
        // plain GetComponents is enough - and it allocates only when one is really asked.
        IHitAbsorber[] absorbers = player.GetComponents<IHitAbsorber>();

        for (int i = 0; i < absorbers.Length; i++)
        {
            if (absorbers[i].TryAbsorbHit(source))
                return true;
        }

        return false;
    }
}
