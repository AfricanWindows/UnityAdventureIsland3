using UnityEngine;

/// <summary>
/// A snake that hops: stands still, jumps forward, lands, stands still again. Always the same
/// way - it never turns around and never mirrors its sprite.
///
/// It decides WHEN to jump. HOW high the jump is, and what gravity does on the way down,
/// belongs to the JumpBehaviour sitting next to it - the very same component the player
/// carries. That is the whole point of splitting the jump in two: the physics was written
/// once, and "on a timer" instead of "on the space bar" is the only thing that differs
/// between a snake and Mario.
///
/// It does not walk: the forward speed is given once, at the push-off, and taken away on
/// landing, so it really stands still between hops.
///
/// It is IActivatable, so a range trigger can let it sleep until the player is close. It does
/// not know the trigger exists and works perfectly well without one: it starts awake, and
/// ActivateNearPlayer is what puts it to sleep, not a setting here.
///
/// Like every enemy here it owns only its behaviour: hurting the player on contact is
/// KillPlayerOnTouch, coming back after being beaten is EnemyRespawnTimer, and health is
/// BaseEnemy. A harmless snake is simply one that carries no touch effect.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundCheck))]
public class HoppingEnemy : BaseEnemy, IActivatable
{
    [Tooltip("Seconds spent standing between hops, counted from the moment it LANDS - so " +
             "the pause is the same whether the hop was long or short.")]
    [SerializeField] private float pauseTime = 1f;

    [Tooltip("Forward speed given at the push-off, in units per second. Together with the " +
             "JumpBehaviour's jump speed this decides how far one hop carries.")]
    [SerializeField] private float hopSpeed = 3f;

    [Tooltip("Which way it hops, for good. -1 = left, 1 = right.")]
    [SerializeField] private float direction = -1f;

    private Rigidbody2D rigid;
    private IGroundCheck groundCheck;
    private JumpBehaviour jump;

    // Awake by default, so a snake with no range trigger simply hops. Only ActivateNearPlayer
    // ever turns this off.
    private bool active = true;

    // Set when it lands, so the pause is a pause on the GROUND and the time spent flying
    // does not eat into it.
    private float nextHopTime;

    // Landing is "was off the ground, is on it now", and that needs the previous answer.
    private bool wasInAir;

    protected override void OnAwake()
    {
        rigid = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<IGroundCheck>();

        // Asked for as the abstract base, exactly as PlayerJump does it, so this class never
        // names a concrete jump and a different one is a different component.
        jump = GetComponent<JumpBehaviour>();

        if (jump == null)
            Debug.LogError("HoppingEnemy: no JumpBehaviour on " + gameObject.name +
                           " - add a Variable Height Jump component.", this);
    }

    /// <summary>
    /// Also runs when a beaten snake is revived, so it always stands for a moment before
    /// hopping again instead of leaping the instant it comes back.
    /// </summary>
    private void OnEnable()
    {
        nextHopTime = Time.time + pauseTime;
        wasInAir = false;
    }

    /// <summary>The player came into range. Stand for one pause, then carry on hopping.</summary>
    public void Activate()
    {
        active = true;
        nextHopTime = Time.time + pauseTime;
    }

    /// <summary>
    /// The player left. It stops starting NEW hops; the one it is in finishes normally,
    /// because freezing an enemy in mid-air looks like a bug.
    /// </summary>
    public void Deactivate()
    {
        active = false;
    }

    private void FixedUpdate()
    {
        if (jump == null || rigid == null || groundCheck == null)
            return;

        if (groundCheck.IsInAir())
        {
            wasInAir = true;
            jump.ApplyAirPhysics(Time.fixedDeltaTime);
            return;
        }

        // Still going up while the collision is still reported: we pushed off a moment ago and
        // physics has not caught up yet. Braking here would kill the hop before it began.
        if (jump.IsRising)
            return;

        if (wasInAir)
        {
            wasInAir = false;
            nextHopTime = Time.time + pauseTime;
        }

        StandStill();

        // Asleep: it still stands properly, it just never starts the next hop.
        if (!active)
            return;

        if (Time.time >= nextHopTime)
            Hop();
    }

    /// <summary>Kills the speed left over from the last hop, so it really stands.</summary>
    private void StandStill()
    {
        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }

    private void Hop()
    {
        // Forward first, then up. The jump component owns the vertical speed and knows nothing
        // about direction, which is what keeps it usable by anything with a Rigidbody2D.
        float forward = direction >= 0f ? 1f : -1f;

        rigid.linearVelocity = new Vector2(forward * hopSpeed, rigid.linearVelocity.y);
        jump.Begin();
    }
}
