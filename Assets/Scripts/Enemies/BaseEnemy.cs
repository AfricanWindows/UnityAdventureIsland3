using Game.Core;
using UnityEngine;

/// <summary>
/// Everything every enemy shares: it can be damaged, it dies, and it kills the player on
/// touch. HOW an enemy behaves is decided by the child class.
///
/// A dead enemy is switched OFF, not destroyed. That single change is what makes the
/// game's rules work without reloading a scene: an enemy beaten in level one is still
/// beaten when the player dies and returns to the start of level one, and it comes back
/// only when the whole game is restarted - which is exactly what ResetToStart is for.
/// Destroying it would have made "come back on restart" impossible.
/// </summary>
public abstract class BaseEnemy : MonoBehaviour, IDamageable, IResettable
{
    [SerializeField] private int health = 1;
    [SerializeField] private string playerTag = "Player";

    // Set only once Awake has actually run. An enemy inside the level that has never been
    // entered has NOT run it - its Awake waits for its container to be switched on - and
    // restoring a "start state" of zero health at the world origin would quietly move every
    // enemy of level two into a heap at (0,0) the first time the player restarts.
    private bool startCaptured;
    private int startHealth;
    private Vector3 startPosition;
    private Quaternion startRotation;

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
        if (amount <= 0)
            return;

        health -= amount;

        if (health <= 0)
            Die();
    }

    /// <summary>Switched off, not destroyed - see the class comment.</summary>
    protected virtual void Die()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called only when the whole game restarts. Health, place and facing all come back,
    /// so an enemy is genuinely as it was when the game was opened.
    /// </summary>
    public virtual void ResetToStart()
    {
        // Never awakened: the object is still exactly as the editor left it, so there is
        // nothing to restore and nothing sensible to restore it to.
        if (!startCaptured)
            return;

        health = startHealth;
        transform.SetPositionAndRotation(startPosition, startRotation);

        // Last: switching the object on runs OnEnable, which is where a subclass expects
        // its state to already be restored.
        gameObject.SetActive(true);
    }

    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null)
            TryKillPlayer(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null)
            TryKillPlayer(col.gameObject);
    }

    private void TryKillPlayer(GameObject other)
    {
        if (!other.CompareTag(playerTag))
            return;

        PlayerDeath playerDeath = other.GetComponent<PlayerDeath>();
        if (playerDeath != null)
            playerDeath.Kill();
    }
}
