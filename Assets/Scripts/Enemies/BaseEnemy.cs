using System;
using Game.Core;
using UnityEngine;

/// <summary>
/// Everything every enemy shares: it can be damaged, and it dies. HOW an enemy behaves is
/// decided by the child class.
///
/// It does not kill the player on contact - that is KillPlayerOnTouch - and it does not
/// decide whether it comes back - that is EnemyRespawnTimer. All this class owns is health
/// and the two states around it (Single Responsibility). An enemy that should stay dead
/// simply carries no timer; a harmless one carries no touch effect.
///
/// A beaten enemy is switched OFF, not destroyed. That single choice is what makes both
/// features possible: the object survives to be brought back by the timer, and it survives
/// to be restored by the whole-game restart. Destroy would have made both impossible.
/// </summary>
public abstract class BaseEnemy : MonoBehaviour, IDamageable, IRespawnable, IResettable
{
    [SerializeField] private int health = 1;

    // Set only once Awake has actually run. An enemy inside a level that has never been
    // entered has NOT run it - its Awake waits for its container to be switched on - and
    // restoring a "start state" of zero health at the world origin would quietly move every
    // enemy of level two into a heap at (0,0) the first time the player restarts.
    private bool startCaptured;
    private int startHealth;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool defeated;

    /// <summary>Raised the moment this enemy is beaten. EnemyRespawnTimer listens.</summary>
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

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || defeated)
            return;

        health -= amount;

        if (health <= 0)
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
    /// Back into the game WHERE IT FELL, with its health restored - what the countdown
    /// timer asks for. The place is deliberately not reset: the assignment says a beaten
    /// enemy returns to the spot where it was beaten.
    /// </summary>
    public virtual void Revive()
    {
        if (!defeated)
            return;

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
