using UnityEngine;

/// <summary>
/// ROLE: While riding, the animal takes any hit instead of the player.
/// PATTERNS: none - implements IHitAbsorber.
/// SOLID: S - split out of PlayerMount.
///
/// "The animal takes the hit": riding one, anything that would hurt the player - a stone, a
/// campfire, an enemy, a shot - costs the animal instead, and the player walks on with his
/// power and his lives untouched.
///
/// It is the player's IHitAbsorber. Every hazard asks "can something take this hit?" before
/// hurting him, and this answers by spending the animal. The obstacle that hit him is smashed
/// in the same breath, because this is the only place that knows BOTH what hit him and that
/// an animal was spent.
///
/// This used to be the third job of PlayerMount, next to holding the animal and firing it.
/// Taking a hit is a rule of its own - when it is allowed, what it smashes, the recovery
/// window after it - so it is a class of its own, and it talks to the saddle only through
/// IMountSlot (Single Responsibility, Dependency Inversion).
/// </summary>
[DisallowMultipleComponent]
public class MountHitAbsorber : MonoBehaviour, IHitAbsorber
{
    private IMountSlot saddle;

    // The fairy, dying, and the recovery window. Asked before the animal is spent - see
    // TryAbsorbHit.
    private IInvincible[] invincibilitySources;

    // Opened when the animal is knocked out from under him - see TryAbsorbHit.
    private IHitRecovery recovery;

    private void Awake()
    {
        saddle = GetComponent<IMountSlot>();

        // Includes the recovery window, and deliberately so: it is what stops a second
        // animal, picked up during that window, from being spent by the same enemy the
        // player is still standing in.
        invincibilitySources = GetComponents<IInvincible>();
        recovery = GetComponent<IHitRecovery>();

        if (saddle == null)
            Debug.LogError("MountHitAbsorber: no IMountSlot on " + gameObject.name + " - there " +
                           "is never an animal to take a hit. Add a Player Mount.", this);

        if (recovery == null)
            Debug.LogWarning("MountHitAbsorber: no IHitRecovery on " + gameObject.name + " - after " +
                             "losing an animal the player can die on the very next step. Add " +
                             "a Hit Invincibility.", this);
    }

    /// <summary>
    /// The animal takes the hit and is gone; the player is not touched at all - no power
    /// lost, no life lost, no knockback.
    ///
    /// The invincibility check is what stops a fairy and an animal from both being spent on
    /// one hit: if something is already protecting him the hit will be refused further down
    /// the line anyway, so the animal stays. It is asked here rather than by the hazard
    /// because only the absorber knows whether it is worth spending itself.
    /// </summary>
    public bool TryAbsorbHit(GameObject source)
    {
        if (saddle == null || !saddle.IsMounted)
            return false;

        if (invincibilitySources.AnyActive())
            return false;

        saddle.Dismount();
        Smash(source);

        // The player is dropped on the exact spot where the thing that took his animal still
        // stands. Without a recovery window the next physics step would kill HIM too - one
        // touch of an enemy would cost the animal and a life together.
        // Opened only here, not on every dismount: stepping off because the game restarted or
        // because the player died is not a hit, and owes him no window.
        if (recovery != null)
            recovery.Begin();

        return true;
    }

    /// <summary>
    /// Riding into an obstacle destroys it as well - the stone and the campfire from the
    /// assignment, both of which already carry a Destructible (an IObstacle) for the fairy.
    ///
    /// It asks for IObstacle and NOT for IForceKillable, and that is the rule rather than
    /// an oversight: enemies answer IForceKillable too, and riding into an enemy must cost
    /// the animal WITHOUT killing the enemy. IObstacle means "an obstacle in the way",
    /// which is exactly the set that gets smashed.
    /// </summary>
    private void Smash(GameObject source)
    {
        if (source == null)
            return;

        // InParent: the collider that hit us is often a child of the object that owns the
        // behaviour.
        IObstacle obstacle = source.GetComponentInParent<IObstacle>();

        if (obstacle != null)
            obstacle.ForceKill();
    }
}
