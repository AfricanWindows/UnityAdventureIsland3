using UnityEngine;

/// <summary>
/// HOW a jump behaves: the push-off, what letting go of the key does, and what gravity does
/// in the air. It knows nothing about WHEN a jump is allowed, who asked for it or which key
/// was pressed - that is PlayerJump's job.
///
/// This is the Template pattern. The base fixes the shape of a jump - three moments, always
/// the same three - and every subclass answers all three itself. None of them has an empty
/// default: the one jump in the game both shortens its rise and falls faster, so such a
/// default would be code that never runs. A jump with one fixed height writes an empty Cut()
/// and says so in plain sight.
///
/// PlayerJump talks to THIS type, never to a concrete jump, so a different feel is a new
/// subclass and not an edit to any existing file (Open/Closed). Nothing here mentions a
/// player, an input device or a tag, so a jumping enemy can carry the same component.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class JumpBehaviour : MonoBehaviour
{
    private Rigidbody2D cachedBody;

    /// <summary>
    /// The body this jump moves. Fetched on first use rather than in an Awake, because Unity
    /// matches message methods by NAME: a subclass that declared its own Awake would silently
    /// replace the base one, and the reference would just never be set.
    /// </summary>
    protected Rigidbody2D Body
    {
        get
        {
            if (cachedBody == null)
                cachedBody = GetComponent<Rigidbody2D>();

            return cachedBody;
        }
    }

    /// <summary>True while still going up. False at the top of the arc and on the way down.</summary>
    public bool IsRising
    {
        get { return Body.linearVelocity.y > 0f; }
    }

    /// <summary>The jump starts. Called once, on the physics step after the key went down.</summary>
    public abstract void Begin();

    /// <summary>
    /// How long this jump stays in the air before coming back down to a given height, measured
    /// from where it took off: 0 = the same height, -2 = two units lower, 1 = one unit higher.
    ///
    /// Only the jump can answer this - it alone knows its push-off speed and what it does to
    /// gravity on the way down - so it is asked HERE, instead of every aiming enemy redoing that
    /// physics with its own copy of the numbers. That is what lets a frog land on a chosen spot
    /// without knowing how its jump works (Information Expert).
    /// </summary>
    public abstract float GetAirTime(float landingHeight);

    /// <summary>
    /// The key was released while still rising, so the player asked for a SHORT jump.
    /// Called at most once per jump. A jump with one fixed height leaves it empty.
    /// </summary>
    public abstract void Cut();

    /// <summary>
    /// Called every fixed step while off the ground - rising, falling, or falling after simply
    /// walking off a ledge. Left empty, it means "plain Unity gravity, as set on the Rigidbody".
    /// </summary>
    public abstract void ApplyAirPhysics(float deltaTime);

    /// <summary>
    /// Writes the vertical speed and leaves the horizontal one alone.
    ///
    /// Setting a speed rather than calling AddForce is deliberate: a force depends on the mass,
    /// on the drag and on how many physics steps it happens to be applied for, so the same jump
    /// reaches a different height on a different machine. A speed is exact, and the height it
    /// buys can be worked out on paper: h = v * v / (2 * 9.81 * gravityScale).
    /// </summary>
    protected void SetVerticalSpeed(float speed)
    {
        Vector2 velocity = Body.linearVelocity;
        velocity.y = speed;
        Body.linearVelocity = velocity;
    }
}
