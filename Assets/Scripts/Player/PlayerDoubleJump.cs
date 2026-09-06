using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Exercise item 8: the second jump Mario can make while already in the air.
///
/// This is a SEPARATE class on purpose. PlayerJump still does exactly what it did
/// before - jump from the ground - and was not modified by a single line: the new
/// ability arrived as a new class (Open/Closed).
///
/// Both scripts read the same key, but their conditions can never both be true:
/// PlayerJump only fires while grounded, this one only while IsInAir() is true.
///
/// The second jump is spent once and comes back the moment Mario lands.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDoubleJump : MonoBehaviour
{
    [Tooltip("Upward force of the second jump. Usually a bit weaker than the first one.")]
    [SerializeField] private float doubleJumpSpeed = 400f;

    private Rigidbody2D rigid;
    private IGroundCheck groundCheck;

    private bool doubleJumpUsed;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<IGroundCheck>();

        if (groundCheck == null)
            Debug.LogError("PlayerDoubleJump: no IGroundCheck on " + gameObject.name, this);
    }

    private void OnEnable()
    {
        // A respawned Mario always starts with his second jump available.
        doubleJumpUsed = false;
    }

    private void Update()
    {
        RefillOnLanding();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            TryDoubleJump();
    }

    /// <summary>Touching the ground gives the second jump back.</summary>
    private void RefillOnLanding()
    {
        if (!groundCheck.IsInAir())
            doubleJumpUsed = false;
    }

    private void TryDoubleJump()
    {
        if (rigid == null)
            return;

        // The extension method answers the single question this feature is built on.
        if (!groundCheck.IsInAir() || doubleJumpUsed)
            return;

        doubleJumpUsed = true;

        // Wipe the falling speed first, otherwise a falling Mario barely rises.
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(new Vector2(0f, doubleJumpSpeed));
    }
}
