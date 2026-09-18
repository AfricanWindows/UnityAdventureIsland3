/// <summary>
/// "It is hiding right now" - the ghost covering its face because the player is looking.
///
/// A pose, read by the view, and nothing else - the same shape as ICrouchState. The ghost
/// decides it; EnemyAnimatorView reads it and never learns what a ghost is (Interface
/// Segregation, Dependency Inversion).
/// </summary>
public interface IHidingState
{
    bool IsHiding { get; }
}
