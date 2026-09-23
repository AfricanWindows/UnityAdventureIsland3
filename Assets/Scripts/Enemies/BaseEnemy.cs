using System;
using Game.Core;
using UnityEngine;

/// <summary>
/// Everything every enemy shares: it can be damaged, and it dies. HOW an enemy behaves is
/// decided by the child class.
///
/// It does not kill the player on contact - that is KillPlayerOnTouch - and it does not decide
/// WHETHER OR WHEN it comes back, which is RespawnTimer's. What it does own is HOW an enemy
/// puts itself back together once somebody asks: its health, its beaten/alive state, and the
/// position it returns to (Single Responsibility). The split is drawn where the knowledge is -
/// only this class remembers where the level first placed it, captured in Awake, and the timer
/// deliberately knows nothing about what it is reviving so that it can serve fruit and eggs too.
/// An enemy that should stay dead simply carries no timer; a harmless one carries no touch effect.
///
/// A beaten enemy is switched OFF, not destroyed. That single choice is what makes both
/// features possible: the object survives to be brought back by the timer, and it survives
/// to be restored by the whole-game restart. Destroy would have made both impossible.
/// </summary>
public abstract class BaseEnemy : MonoBehaviour, IDamageable, IRespawnable, IResettable, IForceKillable
{
    [SerializeField] private int health = 1;

    [Tooltip("No weapon can hurt it: the axe, the boomerang and the animals' attacks all " +
             "bounce off. The fairy still destroys it, because her touch does not come " +
             "through weapon damage at all. Tick it for the ghost.")]
    [SerializeField] private bool weaponProof;

    [Tooltip("Where it comes back after being beaten. OFF = where it fell, which is what the " +
             "assignment asks for. ON = where the level put it - tick this for anything that " +
             "MOVES, or every respawn leaves it a few steps further from home until it has " +
             "crept across the level.\n\n" +
             "On an enemy that does NOT move it changes nothing: it dies where it started, so " +
             "both answers are the same point. That is why the static ones are left unticked " +
             "and every moving one is ticked.")]
    [SerializeField] private bool reviveAtStartPosition;

    // Set only once Awake has actually run. An enemy inside a level that has never been
    // entered has NOT run it - its Awake waits for its container to be switched on - and
    // restoring a "start state" of zero health at the world origin would quietly move every
    // enemy of level two into a heap at (0,0) the first time the player restarts.
    private bool startCaptured;
    private int startHealth;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool defeated;

    /// <summary>Raised the moment this enemy is beaten. RespawnTimer listens.</summary>
    public event Action Defeated;

    public bool IsDefeated { get { return defeated; } }

    // Private on purpose: a subclass that declared its own Awake would silently replace
    // this one, and the starting state would never be captured. Subclasses use OnAwake().
    private void Awake()
    {
        startCaptured = true;
        startHealth = health;
        startPosition = transform.position;
        startRotation = transform.rotation;

        OnAwake();
    }

    /// <summary>Subclass setup. Cache references here, never in Update.</summary>
    protected virtual void OnAwake() { }

    /// <summary>
    /// A weapon's blow. Health is the protection, and Weapon Proof is protection nothing gets
    /// through - the ghost. A checkbox rather than a ghost-only override, so any enemy can be
    /// armoured the same way without a new class.
    /// </summary>
    /// <returns>False when the blow bounced off, so a piercing boomerang stops here instead
    /// of flying on through something it never hurt.</returns>
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || defeated)
            return false;

        if (weaponProof)
            return false;

        health -= amount;

        if (health <= 0)
            Die();

        return true;
    }

    /// <summary>
    /// Wiped out however much health is left - the fairy's touch.
    ///
    /// Written here once, so no enemy in the game needed anything added to it: whatever
    /// answers ForceKill is destroyed, and every enemy answers it through this class. The
    /// respawn timer still hears the same Defeated event, so a fairy-killed enemy comes back
    /// exactly like one beaten with an axe (Open/Closed).
    ///
    /// It goes around TakeDamage on purpose. Health is the protection an enemy has against
    /// WEAPONS, and this is the door that exists for things no protection survives - which
    /// is also how the ghost will work: immune to every weapon, and still gone the moment
    /// the fairy brushes past.
    /// </summary>
    public void ForceKill()
    {
        if (defeated)
            return;

        Die();
    }

    /// <summary>
    /// Switched off, not destroyed. The announcement comes AFTER the object is asleep, so
    /// a listener that asks IsDefeated during the event gets the truth.
    /// </summary>
    protected virtual void Die()
    {
        defeated = true;
        gameObject.SetActive(false);

        if (Defeated != null)
            Defeated();
    }

    /// <summary>
    /// Back into the game with its health restored - what the countdown timer asks for.
    ///
    /// By default it returns to the spot where it was beaten, because the assignment says so.
    /// That answer only works for an enemy that stays put: one that walks or hops dies further
    /// from home every time, so respawning it where it fell would walk it out of its platform
    /// and eventually off the level. Tick Revive At Start Position on those.
    ///
    /// The position it returns to is the one captured in Awake - the one set in the editor -
    /// so there is nothing to type in and nothing to keep in step when the enemy is moved.
    ///
    /// WHAT AN OVERRIDE MUST HONOUR. This method promises only to leave the BODY where it fell.
    /// A subclass that computes its position from a remembered anchor rather than from the body
    /// has to move that anchor too, or the next frame quietly drags the enemy back and the flag
    /// above means nothing - which is exactly what PathEnemy used to do, and why it now overrides
    /// this (Liskov Substitution: the override keeps the promise the base class made).
    /// </summary>
    public virtual void Revive()
    {
        if (!defeated)
            return;

        if (reviveAtStartPosition && startCaptured)
            transform.SetPositionAndRotation(startPosition, startRotation);

        Restore();
    }

    /// <summary>
    /// Called only when the whole game restarts. Unlike Revive, this also puts the enemy
    /// back where the LEVEL starts it, so a patrolling enemy does not begin a new game
    /// wherever it happened to die in the previous one.
    /// </summary>
    public virtual void ResetToStart()
    {
        // Never awakened: the object is still exactly as the editor left it, so there is
        // nothing to restore and nothing sensible to restore it to.
        if (!startCaptured)
            return;

        transform.SetPositionAndRotation(startPosition, startRotation);

        Restore();
    }

    /// <summary>Health back, awake again. The one place either comeback is written.</summary>
    private void Restore()
    {
        defeated = false;
        health = startCaptured ? startHealth : Mathf.Max(1, health);

        // Last: switching the object on runs OnEnable, which is where a subclass
        // expects its state to already be restored.
        gameObject.SetActive(true);
    }
}
