/// <summary>
/// ROLE: Helper: IsInAir() on any IGroundCheck.
/// PATTERNS: none - an extension method on an interface.
/// SOLID: D - extends the interface, so it works with any ground detector.
///
/// Utility methods for IGroundCheck.
///
/// It extends the INTERFACE, not GameObject: there is no GetComponent hidden inside,
/// so calling it every frame costs nothing, and any future way of detecting ground
/// works with it without a change here.
///
/// No state, no game logic - just a question asked in the words the caller is thinking in.
/// PlayerJump asks "is he in the air?" every physics step, and "!groundCheck.IsGrounded" is
/// the same thing said inside out.
/// </summary>
public static class GroundCheckExtensions
{
    /// <summary>True while the owner is not standing on anything solid.</summary>
    public static bool IsInAir(this IGroundCheck groundCheck)
    {
        return groundCheck != null && !groundCheck.IsGrounded;
    }
}
