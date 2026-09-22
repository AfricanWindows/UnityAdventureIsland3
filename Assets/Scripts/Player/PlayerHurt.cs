using UnityEngine;

/// <summary>
/// What happens to the player when a hazard hurts him without killing him: he loses power
/// and he is shoved in the direction he was already going.
///
/// After the hit it opens the recovery window (IHitRecovery): a moment in which nothing
/// can hurt him, shown by a blinking sprite. This class only says WHEN it starts; how long it
/// lasts and how it looks are not its business.
///
/// An earlier version had to do without such a window: a stone that emptied the bar used to
/// lose its life to it, because the empty bar killed through Kill and PlayerDeath refuses Kill
/// while any invincibility is active. The empty bar now uses ForceKill, which nothing can
/// refuse, so the window is safe. "One hit per touch" still lives in PlayerContactEffect.
///
/// WHY THE EXECUTION ORDER. PlayerMovement REWRITES the horizontal velocity every
/// FixedUpdate - it owns walking, and it does not know a shove happened. Setting the
/// velocity once from the hazard therefore did nothing at all: movement overwrote it on
/// the very next physics step. This component runs AFTER movement and re-imposes the shove
/// for its short window, which is the honest way to say "for these few frames something
/// else is driving".
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHurt : MonoBehaviour, IHurtable, IMovementLock, IHurtState
{
    [Tooltip("How long the shove keeps driving the player. Short - this is a stagger, not " +
             "a stun. Steering comes back when it ends.")]
    [SerializeField] private float knockbackSeconds = 0.25f;

    private Rigidbody2D body;
    private IPowerWallet power;
    private IInvincible[] invincibilitySources;
    private IHitRecovery recovery;

    private Vector2 knockbackVelocity;
    private float knockbackUntil;

    /// <summary>True while the shove is still driving him. The animator draws the
    /// hurt pose from this.</summary>
    public bool IsHurt { get { return Time.time < knockbackUntil; } }

    /// <summary>Same window, read by PlayerMovement: he cannot steer out of a shove.</summary>
    public bool BlocksMovement { get { return IsHurt; } }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        power = GetComponentInChildren<IPowerWallet>(true);

        // The fairy, the death animation, and the recovery window after a hit.
        invincibilitySources = GetComponents<IInvincible>();
        recovery = GetComponent<IHitRecovery>();

        if (recovery == null)
            Debug.LogWarning("PlayerHurt: no IHitRecovery on " + gameObject.name + " - a hit " +
                             "opens no recovery window. Add a Hit Invincibility.", this);
    }

    public bool TryHurt(int powerCost, Vector2 knockback)
    {
        // Protected - the fairy, or still recovering from the last hit: ignored entirely.
        if (invincibilitySources.AnyActive())
            return false;

        if (powerCost > 0 && power != null)
            power.RemovePower(powerCost);

        knockbackVelocity = knockback;
        knockbackUntil = Time.time + knockbackSeconds;

        if (body != null)
        {
            // The lift is applied ONCE, here. Re-applying it every step would hold the
            // player in the air like a balloon instead of letting him arc back down.
            body.linearVelocity = knockback;
        }

        if (recovery != null)
            recovery.Begin();

        return true;
    }

    /// <summary>
    /// Runs after PlayerMovement (execution order 100) and puts the shove back, because
    /// movement just overwrote it. Horizontal only: gravity keeps the vertical arc.
    /// </summary>
    private void FixedUpdate()
    {
        if (body == null || Time.time >= knockbackUntil)
            return;

        body.linearVelocity = new Vector2(knockbackVelocity.x, body.linearVelocity.y);
    }
}
