using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the jump input and pushes Mario up. It does not decide what "ground" is -
/// that is GroundCheck's job - so Mario can jump off anything solid, spikes included.
/// </summary>
[RequireComponent(typeof(GroundCheck))]
public class PlayerJump : MonoBehaviour
{
    [Tooltip("Upward force of a jump")]
    [SerializeField] private float jumpSpeed = 100;

    [Tooltip("Stops one key press from firing twice before physics reports us airborne.")]
    [SerializeField] private float jumpCooldown = 0.15f;

    private Rigidbody2D rigid;
    private IGroundCheck groundCheck;

    private float nextJumpTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<IGroundCheck>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Jump();
    }

    private void Jump()
    {
        if (rigid == null || groundCheck == null)
            return;

        if (!groundCheck.IsGrounded || Time.time < nextJumpTime)
            return;

        nextJumpTime = Time.time + jumpCooldown;
        rigid.AddForce(new Vector2(0, jumpSpeed));
    }
}
