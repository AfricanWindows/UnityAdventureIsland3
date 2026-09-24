using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Moves the player left and right.
///
/// It OWNS its speed: no other class writes into the field from outside.
///
/// He does not reach that speed instantly. Snapping straight to the maximum reads as a
/// sprite being teleported rather than a character starting to walk, so the speed ramps
/// up and brakes down at rates the designer sets.
///
/// It does not read a keyboard. The intention "he wants to go right" arrives through
/// IInputSource, injected by GameInstaller, so this class works unchanged with a gamepad
/// or inside a test (Dependency Inversion).
/// </summary>
public class PlayerMovement : InputDrivenBehaviour, IFacing, IMovementSpeed
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

    private float facingDirection = 1f;
    private float direction;

    // His own walking speed, moved one step of the ramp towards the target every physics step.
    private float ownSpeedX;

    private Rigidbody2D rigid;

    // Every reason the player might not be allowed to walk, collected once. The array is
    // what makes this open: a new lock is a new component, never an edit here.
    private IMovementLock[] movementLocks;

    /// <summary>
    /// His own walking speed - what the run animation reads, so the run cycle follows the
    /// steps he takes rather than anything else that happens to move his body.
    /// </summary>
    public float OwnSpeedX
    {
        get { return ownSpeedX; }
    }

    /// <summary>Which way the player looks right now. Weapons aim by this, not by the scale.</summary>
    public float FacingDirection
    {
        get { return facingDirection; }
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        movementLocks = GetComponents<IMovementLock>();
    }

    /// <summary>
    /// Switched off - PlayerDeath does it for the length of the death animation - he stands.
    /// Without this the speed he died with outlived the death: back on the spawn point, the
    /// first physics steps carried him a little forward and the run cycle flickered on.
    /// </summary>
    private void OnDisable()
    {
        ownSpeedX = 0f;
    }

    private void FixedUpdate()
    {
        ReadInput();
        ApplyMovement();
    }

    /// <summary>
    /// One line, because deciding which key means "left" is not this class's job.
    /// </summary>
    private void ReadInput()
    {
        direction = HasInput ? InputSource.Horizontal : 0f;
    }

    private void ApplyMovement()
    {
        if (rigid == null)
            return;

        // Locked: he brakes to a stop, but the facing code below still runs, so holding S
        // and pressing left or right turns him on the spot without moving him.
        float targetOwnSpeed = IsMovementBlocked ? 0f : direction * speed;

        // One step of the ramp. MoveTowards never overshoots, so releasing the key lands
        // on exactly 0 instead of jittering around it.
        ownSpeedX = Mathf.MoveTowards(ownSpeedX, targetOwnSpeed,
                                      ChooseRate(ownSpeedX, targetOwnSpeed) * Time.fixedDeltaTime);

        // The horizontal velocity is REWRITTEN every step rather than added to, which is also
        // why friction can never drag him a second time.
        rigid.linearVelocity = new Vector2(ownSpeedX, rigid.linearVelocity.y);

        // Which way he FACES changes instantly, even though his speed ramps: the ramp is about
        // how fast he moves, and looking the wrong way for a tenth of a second reads as broken.
        // Only the answer is decided here - turning the picture round is FacingView's job, the
        // same component the ghost uses.
        if (direction != 0f)
            facingDirection = direction > 0f ? 1f : -1f;
    }

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

    /// <summary>
    /// Speeding up uses one rate, slowing down another. Turning around counts as braking
    /// until the speed passes through zero, which is what makes a change of direction feel
    /// like a real turn instead of a slow drift across the middle.
    /// </summary>
    private float ChooseRate(float current, float target)
    {
        bool sameWay = current == 0f || target == 0f || Mathf.Sign(current) == Mathf.Sign(target);
        bool speedingUp = sameWay && Mathf.Abs(target) > Mathf.Abs(current);

        return speedingUp ? acceleration : deceleration;
    }
}
