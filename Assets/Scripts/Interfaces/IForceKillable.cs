/// <summary>
/// "Be destroyed, and do not argue."
///
/// The second and last way anything in this game is removed, next to IDamageable. The
/// difference is the whole point:
///
///   IDamageable.TakeDamage - a blow that PROTECTION MAY SURVIVE. An axe, a boomerang, an
///                            animal's fire. The target keeps its health and its rules.
///   IForceKillable.ForceKill - an end that nothing survives. The fairy touching an enemy,
///                            a campfire or a stone; the abyss swallowing the player.
///
/// Two interfaces rather than a "ignoresProtection" flag on one, because the flag would put
/// the decision in the hands of every caller and every prefab in the level - one wrong tick
/// on a snake and the fairy quietly stops working. Here the rule is the TYPE the caller
/// asks for, so who may ignore protection is a question the code answers, not the scene
/// (Interface Segregation, and the same reasoning that keeps KillPlayerOnTouch and
/// HurtPlayerOnTouch two components instead of one with a mode).
///
/// One method, so a campfire is not also handed health, respawning or a death event it has
/// no use for.
/// </summary>
public interface IForceKillable
{
    void ForceKill();
}
