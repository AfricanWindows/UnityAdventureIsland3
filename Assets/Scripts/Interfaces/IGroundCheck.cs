/// <summary>
/// Answers one question: is the owner standing on something solid?
/// PlayerJump depends on this abstraction, not on a concrete detection method.
/// </summary>
public interface IGroundCheck
{
    bool IsGrounded { get; }
}
