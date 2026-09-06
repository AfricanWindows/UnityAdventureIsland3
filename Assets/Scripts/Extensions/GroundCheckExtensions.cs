/// <summary>
/// Utility methods for IGroundCheck (exercise item 8).
///
/// It extends the INTERFACE, not GameObject: there is no GetComponent hidden inside,
/// so calling it every frame costs nothing, and any future way of detecting ground
/// works with it without a change here.
///
/// No state, no game logic - just a question asked in a readable way.
/// </summary>
public static class GroundCheckExtensions
{
    /// <summary>True while the owner is not standing on anything solid.</summary>
    public static bool IsInAir(this IGroundCheck groundCheck)
    {
        return groundCheck != null && !groundCheck.IsGrounded;
    }
}
