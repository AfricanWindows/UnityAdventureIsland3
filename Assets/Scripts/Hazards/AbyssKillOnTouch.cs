using UnityEngine;

/// <summary>
/// The bottomless pit: falling in ends the run, fairy or no fairy.
///
/// It is the twin of KillPlayerOnTouch and differs in exactly one line - the interface it
/// asks for:
///
///   KillPlayerOnTouch  -> IKillable.Kill()       "die, if nothing is protecting you"
///   AbyssKillOnTouch   -> IForceKillable.ForceKill()  "die, full stop"
///
/// That is the whole of the exception the assignment asks for, and it is stated once, in
/// the only object that is allowed to make it. There is no "ignores invincibility" tick box
/// anywhere in the game, so no snake, campfire or stone can ever be given that power by a
/// slip in the Inspector (Open/Closed - the rule is extended by adding this component, never
/// by editing the ones that already work).
///
/// Finding the player, filtering by tag and firing once per touch are all inherited from
/// PlayerContactEffect, the same skeleton the stone and the campfire use.
///
/// Meant for a wide trigger volume stretched along the bottom of a level, and for the gaps
/// inside the second level's maze. A zone rather than a floor on purpose: the player should
/// fall properly out of sight and die down there, not stop dead at the edge.
///
/// It stays deliberately blind to everything but the player. An enemy that hops into a pit
/// is not this object's problem - and if it ever becomes one, the answer is a second small
/// component, not a flag here.
/// </summary>
public class AbyssKillOnTouch : PlayerContactEffect
{
    protected override void Affect(GameObject player)
    {
        IForceKillable victim = player.GetComponent<IForceKillable>();

        if (victim != null)
            victim.ForceKill();
    }
}
