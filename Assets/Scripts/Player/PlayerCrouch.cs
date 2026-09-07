using Game.Core.Controls;

/// <summary>
/// Lying down: hold Down or S and the player drops to the ground, stops walking, but can
/// still turn to face the other way - exactly as in Adventure Island.
///
/// It is a SEPARATE component, not a branch inside PlayerMovement. Movement keeps its one
/// job (how fast he walks) and gains no knowledge of poses; this class keeps its one job
/// (am I lying down) and gains no knowledge of acceleration (Single Responsibility).
///
/// It publishes that state twice, through two small interfaces, because two different
/// things want it for two different reasons:
///   IMovementLock - PlayerMovement, to refuse to walk
///   ICrouchState  - PlayerAnimatorView, to draw the lying sprite
/// Neither of them references this class by name, so the day crouching is replaced by a
/// slide, or granted by an item, nothing else changes.
///
/// Facing is NOT handled here on purpose. PlayerMovement already owns which way the player
/// looks, and it keeps turning him while locked - so holding S and pressing left or right
/// flips the sprite without moving him, which is the behaviour asked for.
/// </summary>
public class PlayerCrouch : InputDrivenBehaviour, IMovementLock, ICrouchState
{
    /// <summary>True while the key is held down.</summary>
    public bool IsCrouching { get; private set; }

    /// <summary>Lying down is a reason not to walk.</summary>
    public bool BlocksMovement { get { return IsCrouching; } }

    // Update, not FixedUpdate: this is a state read by rendering as well as by physics,
    // and a pose that lags a frame behind the key is visible.
    private void Update()
    {
        IsCrouching = HasInput && InputSource.CrouchHeld;
    }

    private void OnDisable()
    {
        // A disabled crouch must not leave the player frozen forever.
        IsCrouching = false;
    }
}
