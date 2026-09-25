/// <summary>
/// ROLE: "Am I standing on something solid?"
/// PATTERNS: none - a role interface.
/// SOLID: D - PlayerJump and HoppingEnemy never name GroundCheck.
///
/// Answers one question: is the owner standing on something solid?
/// PlayerJump depends on this abstraction, not on a concrete detection method.
/// </summary>
public interface IGroundCheck
{
    bool IsGrounded { get; }
}
