using Game.Core;
using UnityEngine;

/// <summary>
/// ROLE: Lets the fairy or a ridden animal wipe an obstacle (stone, campfire) out of the level.
/// PATTERNS: none - implements IObstacle and IResettable.
/// SOLID: O - one component on the prefab; it comes back on a restart.
///
/// "The fairy can wipe this out of the level" - the campfire and the stone.
///
/// Enemies do not need it: BaseEnemy already knows how to die, so it answers ForceKill
/// itself. This exists for everything that is NOT an enemy - an obstacle with no health, no
/// respawn timer and nothing to hurt.
///
/// It also answers the question "why does the floor not vanish when the fairy walks on it".
/// Nothing is destructible by default; carrying this component IS the permission, which is
/// one drag in the Inspector and impossible to trigger by accident (Open/Closed).
///
/// Switched off, not destroyed - the same choice BaseEnemy makes, and for the same reason:
/// the object has to survive to be brought back by the whole-game restart, and Destroy would
/// have made that impossible.
///
/// It deliberately knows nothing about fairies. Anything that gets hold of an IForceKillable
/// can break it, so a future bomb or a boss's stomp needs no change here (Dependency
/// Inversion).
///
/// It is an IObstacle, which IS an IForceKillable - so the fairy still finds it, and riding
/// an animal into it can tell it apart from an enemy without naming this class.
/// </summary>
[DisallowMultipleComponent]
public class Destructible : MonoBehaviour, IObstacle, IResettable
{
    public void ForceKill()
    {
        if (!gameObject.activeSelf)
            return;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// A new game puts it back. Dying does NOT - a campfire the fairy already cleared stays
    /// cleared when the player walks the level again, the same rule eaten fruit follows.
    /// </summary>
    public void ResetToStart()
    {
        gameObject.SetActive(true);
    }
}
