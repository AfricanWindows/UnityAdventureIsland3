using UnityEngine;

/// <summary>
/// The Adventure Island jump: a tap lifts the player a little, holding the key lifts him high.
///
/// It does NOT charge up while the key is held - that would make the push-off wait for the
/// player and feel late. It does what the NES platformers did: the push-off is always the FULL
/// one, and letting go early cuts the rise short. So the jump starts on the very frame the key
/// goes down, and the player still decides how high it goes.
///
/// Falling then runs at a heavier gravity than rising. Real gravity is symmetric and reads as
/// floaty; going up slowly and coming down fast is what makes a jump feel crisp.
/// </summary>
public class VariableHeightJump : JumpBehaviour
{
    [Tooltip("Upward speed given the moment the key goes down, in units per second. This is " +
             "the FULL jump, the one you get by holding the key. Height reached is roughly " +
             "JumpSpeed * JumpSpeed / (2 * 9.81 * the Rigidbody's Gravity Scale).")]
    [SerializeField] private float jumpSpeed = 13f;

    [Tooltip("What is left of the upward speed when the key is released while still rising. " +
             "0 = stops dead and drops, 1 = releasing changes nothing. Height scales with the " +
             "SQUARE of this, so 0.4 gives about a sixth of the full jump.")]
    [Range(0f, 1f)]
    [SerializeField] private float cutMultiplier = 0.4f;

    [Tooltip("How much heavier gravity is while falling than while rising. 1 = normal " +
             "symmetric physics. Around 1.8 makes the jump feel snappy rather than floaty. " +
             "It applies to walking off a ledge too, which is what you want.")]
    [Min(1f)]
    [SerializeField] private float fallGravityMultiplier = 1.8f;

    public override void Begin()
    {
        // Overwriting the speed rather than adding to it means a jump is the same height
        // whether he was walking, already falling a little, or riding a lift upwards.
        SetVerticalSpeed(jumpSpeed);
    }

    public override void Cut()
    {
        // Past the top of the arc there is no rise left to shorten, and cutting a negative
        // speed would slow the FALL down - a released key would make him hover.
        if (!IsRising)
            return;

        SetVerticalSpeed(Body.linearVelocity.y * cutMultiplier);
    }

    public override void ApplyAirPhysics(float deltaTime)
    {
        // Rising keeps the Rigidbody's own gravity: the extra weight is for the way down.
        if (Body.linearVelocity.y >= 0f)
            return;

        // Unity already applied one helping of gravity this step, so only the DIFFERENCE is
        // added here - which is why a multiplier of 1 adds nothing at all. Physics2D.gravity.y
        // is negative, so this subtracts speed and he falls faster.
        float extraGravity = Physics2D.gravity.y * Body.gravityScale *
                             (fallGravityMultiplier - 1f) * deltaTime;

        SetVerticalSpeed(Body.linearVelocity.y + extraGravity);
    }
}
