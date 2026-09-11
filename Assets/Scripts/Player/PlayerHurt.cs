using UnityEngine;

/// <summary>
/// What happens to the player when a hazard hurts him without killing him: he loses power
/// and he is shoved in the direction he was already going.
///
/// There is NO recovery window here, and that is deliberate. The first version had one -
/// an invincibility timer - and it created two problems for one it solved: the number had
/// to be tuned by feel, and a stone that emptied the bar refilled it and then silently
/// skipped the life it should have cost, because PlayerDeath refuses to kill while any
/// invincibility is active. "One hit per touch" lives in PlayerContactEffect instead, where
/// it is a fact about the contact rather than a timer about the player.
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
public class PlayerHurt : MonoBehaviour, IHurtable, IMovementLock
{
    [Tooltip("How long the shove keeps driving the player. Short - this is a stagger, not " +
             "a stun. Steering comes back when it ends.")]
    [SerializeField] private float knockbackSeconds = 0.25f;

    private Rigidbody2D body;
    private PowerController power;
    private IInvincible[] invincibilitySources;

    private Vector2 knockbackVelocity;
    private float knockbackUntil;

    /// <summary>
    /// True for the shove only. It stops the player from steering out of it, and stops
    /// walking speed from building up meanwhile.
    /// </summary>
    /// <summary>True while the shove is still driving him. The animator draws the
    /// hurt pose from this.</summary>
    public bool IsHurt { get { return Time.time < knockbackUntil; } }

    /// <summary>Same window, read by PlayerMovement: he cannot steer out of a shove.</summary>
    public bool BlocksMovement { get { return IsHurt; } }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        power = GetComponentInChildren<PowerController>(true);

        // The star, and the fairy later.
        invincibilitySources = GetComponents<IInvincible>();
    }

    public bool TryHurt(int powerCost, Vector2 knockback)
    {
        // Genuinely invincible - the star, the fairy: the hit is ignored entirely.
        if (IsProtected())
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

    private bool IsProtected()
    {
        for (int i = 0; i < invincibilitySources.Length; i++)
        {
            if (invincibilitySources[i].IsInvincible)
                return true;
        }

        return false;
    }
}
