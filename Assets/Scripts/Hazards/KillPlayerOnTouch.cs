using UnityEngine;

/// <summary>
/// Touching this kills the player outright: enemies, spikes, and the campfire from the
/// assignment, which is described as instant death.
///
/// It asks for IKillable, so it never learns what PlayerDeath is, how respawning works, or
/// that a star can make the player immune - PlayerDeath answers all of that behind the
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
        IKillable killable = player.GetComponent<IKillable>();

        if (killable != null)
            killable.Kill();
    }
}
