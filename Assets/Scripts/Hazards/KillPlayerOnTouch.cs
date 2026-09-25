using UnityEngine;

/// <summary>
/// ROLE: Touching this kills the player (enemies, spikes, the campfire).
/// PATTERNS: Template Method - fills Affect of PlayerContactEffect.
/// SOLID: D - talks to IKillable.
///
/// Touching this kills the player outright: enemies, spikes, and the campfire from the
/// assignment, which is described as instant death.
///
/// It asks for IKillable, so it never learns what PlayerDeath is, how respawning works, or
/// that the fairy can make the player immune - PlayerDeath answers all of that behind the
/// interface (Dependency Inversion).
///
/// It is a COMPONENT rather than a base class on purpose. An enemy is not "a kind of
/// hazard"; it is a thing that can be hurt which also happens to hurt on contact. Composing
/// the two keeps a future harmless enemy, or a hazard that cannot be damaged, one component
/// away instead of one inheritance chain away.
/// </summary>
public class KillPlayerOnTouch : PlayerContactEffect
{
    protected override void Affect(GameObject player)
    {
        // Something on the player may take the hit instead - the animal he is riding
        // disappears, he walks on untouched, and an obstacle like this campfire is smashed
        // in the process. Asked here, before the killing, because a hit that was taken never
        // reaches him at all.
        if (player.TryAbsorbHit(gameObject))
            return;

        IKillable killable = player.GetComponent<IKillable>();

        if (killable != null)
            killable.Kill();
    }
}
