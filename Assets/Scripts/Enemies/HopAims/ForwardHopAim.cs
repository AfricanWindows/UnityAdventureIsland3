using UnityEngine;

/// <summary>
/// Always the same way, always the same distance - the snake.
/// </summary>
public class ForwardHopAim : HopAim
{
    [Tooltip("Which way it hops, for good. -1 = left, 1 = right.")]
    [SerializeField] private float direction = -1f;

    [Tooltip("Forward speed given at the push-off, in units per second. Together with the " +
             "jump's Jump Speed this decides how far one hop carries.")]
    [SerializeField] private float hopSpeed = 3f;

    public override float GetHorizontalSpeed(JumpBehaviour jump)
    {
        return GetFacing() * hopSpeed;
    }

    /// <summary>Always the way it hops - so the snake can never hop backwards.</summary>
    public override float GetFacing()
    {
        return direction >= 0f ? 1f : -1f;
    }
}
