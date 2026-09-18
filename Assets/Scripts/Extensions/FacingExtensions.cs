using UnityEngine;

/// <summary>
/// "Which way is that from here - left or right?"
///
/// Written once because three unrelated places ask it: the ghost turning to the player, the
/// ghost checking whether the player looks at IT, and the frog watching the player before a
/// hop. The dead zone is the part worth sharing - without it a ghost hovering straight above
/// the player would flip left and right every step as the two x values crossed (Don't Repeat
/// Yourself).
/// </summary>
public static class FacingExtensions
{
    // Closer than this, sideways, counts as "straight above or below" - no side at all.
    private const float DeadZone = 0.1f;

    /// <returns>+1 if the target is to the right, -1 to the left, 0 if it is too close to
    /// call - the caller then keeps whatever it was doing.</returns>
    public static float HorizontalDirectionTo(this Transform from, Vector3 target)
    {
        float dx = target.x - from.position.x;

        if (Mathf.Abs(dx) < DeadZone)
            return 0f;

        return dx > 0f ? 1f : -1f;
    }
}
