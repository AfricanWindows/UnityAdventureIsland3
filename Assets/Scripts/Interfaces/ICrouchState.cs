/// <summary>
/// ROLE: "The player is lying down" - read by the animator view.
/// PATTERNS: none - a state interface.
/// SOLID: I - one property.
///
/// "The player is lying down right now."
///
/// Deliberately separate from IMovementLock. They answer different questions: the lock
/// says WHETHER he may walk, this says WHICH POSE he is in. The animator needs the pose
/// and must not draw the lying sprite because some unrelated lock - a cut-scene, a stun -
/// happens to be holding him still (Interface Segregation).
/// </summary>
public interface ICrouchState
{
    bool IsCrouching { get; }
}
