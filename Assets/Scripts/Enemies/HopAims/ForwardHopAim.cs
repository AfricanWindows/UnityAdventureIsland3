using UnityEngine;

/// <summary>
/// Always to the left, always the same distance - the snake.
/// </summary>
public class ForwardHopAim : HopAim
{
    [Tooltip("Speed to the left given at the push-off, in units per second. Together with the " +
             "jump's Jump Speed this decides how far one hop carries.")]
    [Min(0f)]
    [SerializeField] private float hopSpeed = 3f;

    public override float GetHorizontalSpeed(JumpBehaviour jump)
    {
        return -hopSpeed;
    }
}
