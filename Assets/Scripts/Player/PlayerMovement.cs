using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Moves Mario left and right, and carries him along when the floor under his feet
/// is a moving platform.
///
/// It OWNS its speed: no other class writes into the field from outside. A temporary
/// boost is asked for through SetSpeedMultiplier, so the lightning effect never has to
/// know what the normal speed is, or remember to put it back.
///
/// He does not reach that speed instantly. Snapping straight to the maximum reads as a
/// sprite being teleported rather than a character starting to walk, so the speed ramps
/// up and brakes down at rates the designer sets.
///
/// It no longer reads a keyboard. The intention "he wants to go right" arrives through
/// IInputSource, injected by GameInstaller, so this class works unchanged with a gamepad
/// or inside a test (Dependency Inversion).
/// </summary>
public class PlayerMovement : InputDrivenBehaviour, IFacing
{
    [Tooltip("Normal walking speed, before any power up")]
    [SerializeField] private float speed = 5f;

    [Header("How the speed is reached")]
    [Tooltip("Units per second gained while a key is held. Lower = softer start. " +
             "Time to full speed is roughly Speed / Acceleration.")]
    [SerializeField] private float acceleration = 40f;

    [Tooltip("Units per second lost when braking or turning around. Usually higher than " +
             "Acceleration - stopping should feel sharper than starting.")]
    [SerializeField] private float deceleration = 60f;

    private float speedMultiplier = 1f;
    private float facingDirection = 1f;
    private float direction;

    // Mario's OWN horizontal speed, measured against the floor he stands on - not the
    // world. Keeping it separate from the platform's speed is what lets the ramp work
    // while riding a moving platform: he accelerates relative to the floor, exactly as
    // he would on solid ground.
    private float ownSpeedX;

    private Rigidbody2D rigid;
    private IPlatformProvider platformProvider;

    // Every reason the player might not be allowed to walk, collected once. The array is
    // what makes this open: a new lock is a new component, never an edit here.
    private IMovementLock[] movementLocks;

    /// <summary>Speed actually used right now: normal speed times the active multiplier.</summary>
    public float CurrentSpeed
    {
        get { return speed * speedMultiplier; }
    }

    /// <summary>
    /// His OWN horizontal speed, measured against the floor he stands on - not against the
    /// world. This is what an animation must read: on a moving platform the Rigidbody says
    /// he is travelling at 3 units per second while he is in fact standing still, and a run
    /// cycle driven by that would play while he does not take a single step.
    /// </summary>
    public float OwnSpeedX
    {
        get { return ownSpeedX; }
    }

    /// <summary>Which way Mario looks right now. Weapons aim by this, not by the scale.</summary>
    public float FacingDirection
    {
        get { return facingDirection; }
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        platformProvider = GetComponent<IPlatformProvider>();
        movementLocks = GetComponents<IMovementLock>();
    }

    /// <summary>
    /// Used by timed effects. 1.5 means "+50% while the effect lasts", 1 means normal.
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier > 0f ? multiplier : 1f;
    }

    private void FixedUpdate()
    {
        ReadInput();
        ApplyMovement();
    }

    /// <summary>
    /// One line, because deciding which key means "left" is no longer this class's job.
    /// </summary>
    private void ReadInput()
    {
        direction = HasInput ? InputSource.Horizontal : 0f;
    }

    private void ApplyMovement()
    {
        if (rigid == null)
            return;

        float platformSpeedX = GetPlatformSpeedX();
        // Locked: he brakes to a stop, but the facing code below still runs, so holding S
        // and pressing left or right turns him on the spot without moving him.
        float targetOwnSpeed = IsMovementBlocked ? 0f : direction * CurrentSpeed;

        // One step of the ramp. MoveTowards never overshoots, so releasing the key lands
        // on exactly 0 instead of jittering around it.
        ownSpeedX = Mathf.MoveTowards(ownSpeedX, targetOwnSpeed,
                                      ChooseRate(ownSpeedX, targetOwnSpeed) * Time.fixedDeltaTime);

        // Walking ON a platform means platform speed PLUS his own steps. Standing still on
        // one means matching its speed exactly, so he keeps the spot he is standing on -
        // both cases fall out of the same line, because ownSpeedX is simply 0 when idle.
        // The velocity is REWRITTEN every step, which is also why friction can no longer
        // drag him a second time.
        rigid.linearVelocity = new Vector2(platformSpeedX + ownSpeedX, rigid.linearVelocity.y);

        // Turning the sprite stays instant. The ramp is about how fast he MOVES; making
        // him look the wrong way for a tenth of a second just reads as broken.
        if (direction != 0f)
        {
            facingDirection = direction > 0f ? 1f : -1f;
            transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }

    /// <summary>
    /// Speeding up uses one rate, slowing down another. Turning around counts as braking
    /// until the speed passes through zero, which is what makes a change of direction feel
    /// like a real turn instead of a slow drift across the middle.
    /// </summary>
    /// <summary>
    /// True while ANY lock is holding him - crouching today, a stun or a cut-scene
    /// tomorrow. Nothing here knows what those are (Dependency Inversion).
    /// </summary>
    private bool IsMovementBlocked
    {
        get
        {
            for (int i = 0; i < movementLocks.Length; i++)
            {
                if (movementLocks[i].BlocksMovement)
                    return true;
            }

            return false;
        }
    }

    private float ChooseRate(float current, float target)
    {
        bool sameWay = current == 0f || target == 0f || Mathf.Sign(current) == Mathf.Sign(target);
        bool speedingUp = sameWay && Mathf.Abs(target) > Mathf.Abs(current);

        return speedingUp ? acceleration : deceleration;
    }

    /// <summary>
    /// Horizontal speed of the floor under our feet, or 0 on normal ground.
    /// The platform is only asked HOW FAR it moved - it never pushes us itself,
    /// so a lift or a conveyor needs no change here.
    /// </summary>
    private float GetPlatformSpeedX()
    {
        if (platformProvider == null)
            return 0f;

        IRideablePlatform platform = platformProvider.CurrentPlatform;
        if (platform == null)
            return 0f;

        return platform.Delta.x / Time.fixedDeltaTime;
    }
}
