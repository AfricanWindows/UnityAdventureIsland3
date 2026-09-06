using UnityEngine;

/// <summary>
/// VIEW. Turns what the player is already doing into Animator parameters, and does
/// nothing else: it never moves him, never reads input, never decides when he may run.
///
/// That split is the whole point. PlayerMovement stays the single owner of the movement
/// rules and does not gain an Animator field; adding, removing or reworking the animation
/// touches only this file (Single Responsibility). A character with no Animator simply
/// does not carry this component.
///
/// It reads PlayerMovement.OwnSpeedX rather than the Rigidbody. On a moving platform the
/// Rigidbody reports the platform's speed as if it were his, so a run cycle driven by it
/// would play while he stands still on a lift.
/// </summary>
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class PlayerAnimatorView : MonoBehaviour
{
    [Header("Animator parameters")]
    [Tooltip("Float. Absolute walking speed, 0 while standing. This is the one the " +
             "Idle/Run transitions read.")]
    [SerializeField] private string speedParameter = "Speed";

    [Tooltip("Optional Bool, empty by default. Fill this in only if a jump or fall state " +
             "is ever added - with just Idle and Run there is nothing for it to switch.")]
    [SerializeField] private string groundedParameter = "";

    [Header("Tuning")]
    [Tooltip("Below this speed he counts as standing. Stops the run cycle from flickering " +
             "on during the last fraction of the braking ramp.")]
    [SerializeField] private float runThreshold = 0.05f;

    private Animator animator;
    private PlayerMovement movement;
    private IGroundCheck groundCheck;

    // Hashed once. Animator.SetFloat("Speed", ...) looks the name up by string on every
    // call, every frame; the int overload does not.
    private int speedHash;
    private int groundedHash;

    private bool hasSpeed;
    private bool hasGrounded;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        groundCheck = GetComponent<IGroundCheck>();

        // An empty name means "this character has no such state", so an Idle/Run-only
        // controller never gets asked for a parameter it does not declare - which is what
        // Unity warns about, once per frame, forever.
        hasSpeed = !string.IsNullOrEmpty(speedParameter);
        hasGrounded = !string.IsNullOrEmpty(groundedParameter) && groundCheck != null;

        if (hasSpeed)
            speedHash = Animator.StringToHash(speedParameter);

        if (hasGrounded)
            groundedHash = Animator.StringToHash(groundedParameter);

        if (movement == null)
            Debug.LogError("PlayerAnimatorView: no PlayerMovement on " + gameObject.name, this);
    }

    private void Update()
    {
        if (movement == null)
            return;

        if (hasSpeed)
        {
            float speed = Mathf.Abs(movement.OwnSpeedX);
            animator.SetFloat(speedHash, speed < runThreshold ? 0f : speed);
        }

        if (hasGrounded)
            animator.SetBool(groundedHash, groundCheck.IsGrounded);
    }
}
