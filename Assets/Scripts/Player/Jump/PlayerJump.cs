using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Decides WHEN the player may jump, and leaves the jump itself to a JumpBehaviour.
///
/// It knows two things and nothing else: the player asked (IInputSource) and his feet are on
/// something solid (IGroundCheck). It does not know how high a jump is, whether releasing the
/// key means anything, or what gravity does on the way down - so retuning the feel of the jump,
/// or replacing it outright, never touches this file (Single Responsibility, Open/Closed).
///
/// It does not decide what "ground" is either, so the player can jump off anything solid,
/// spikes included, and it does not decide which KEY means jump - that is IInputSource's job.
///
/// The double jump that used to sit next to this class is gone: the final assignment does not
/// ask for it, and a second jump in mid-air is not how Adventure Island plays.
///
/// WHY THE WORK IS SPLIT OVER TWO UPDATE METHODS
/// Update runs once per rendered frame and is the only place a "was pressed this frame" event
/// can be caught - FixedUpdate can run twice, or not at all, in the same frame and would drop
/// presses. FixedUpdate is the only place physics may be touched, because that IS the physics
/// step. So Update records the intention and FixedUpdate acts on it. The old code pushed with
/// AddForce straight from Update, which is why it needed a cooldown to stop double firing and
/// why the jump was as high as the frame rate happened to allow.
/// </summary>
[RequireComponent(typeof(GroundCheck))]
public class PlayerJump : InputDrivenBehaviour
{
    private IGroundCheck groundCheck;
    private JumpBehaviour jump;

    // Set by Update, consumed by the next physics step. A press is never lost and never used
    // twice, whatever the frame rate does. This is also why no cooldown is needed any more.
    private bool jumpRequested;

    // True while the rise of the current jump can still be cut short by letting go. It ends at
    // the top of the arc or the moment the key is released, whichever comes first.
    private bool riseIsCuttable;

    private void Awake()
    {
        groundCheck = GetComponent<IGroundCheck>();

        // Asked for as the abstract base, so this class never names a concrete jump. Swapping
        // the feel of the jump is swapping the component, not editing anything here.
        jump = GetComponent<JumpBehaviour>();

        if (jump == null)
            Debug.LogError("PlayerJump: no JumpBehaviour on " + gameObject.name +
                           " - add a Variable Height Jump component.", this);
    }

    /// <summary>A respawned player must not inherit the half-finished jump he died in.</summary>
    private void OnEnable()
    {
        jumpRequested = false;
        riseIsCuttable = false;
    }

    private void Update()
    {
        if (HasInput && InputSource.JumpPressed)
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        if (jump == null || groundCheck == null)
            return;

        if (jumpRequested)
        {
            jumpRequested = false;
            TryBeginJump();
        }

        UpdateRise();

        // Runs while falling too, including a fall that was never a jump - walking off a ledge
        // should feel as heavy as coming down from a jump, or the two read as different worlds.
        if (groundCheck.IsInAir())
            jump.ApplyAirPhysics(Time.fixedDeltaTime);
    }

    private void TryBeginJump()
    {
        if (!groundCheck.IsGrounded)
            return;

        riseIsCuttable = true;
        jump.Begin();
    }

    /// <summary>
    /// Watches the one thing that makes the height variable: whether the key is still held.
    /// Asked as a state and not as an event, so a release that happens between two physics
    /// steps is still noticed on the next one.
    /// </summary>
    private void UpdateRise()
    {
        if (!riseIsCuttable)
            return;

        // Past the top of the arc there is nothing left to shorten. Closing the window here is
        // what stops a key released while already falling from slowing the fall down.
        if (!jump.IsRising)
        {
            riseIsCuttable = false;
            return;
        }

        // No input source at all: leave the jump at full height rather than cutting short a
        // jump the player never got the chance to hold.
        if (!HasInput || InputSource.JumpHeld)
            return;

        // Let go early - this is the short hop.
        riseIsCuttable = false;
        jump.Cut();
    }
}
