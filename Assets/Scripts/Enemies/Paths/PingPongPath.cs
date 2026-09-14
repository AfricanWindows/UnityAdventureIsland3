using UnityEngine;

/// <summary>
/// There and back along a straight line, forever - the spider on its thread.
///
/// The line is a direction rather than "up or down", so the same route serves a spider going
/// down from a ceiling, one going up from a floor, or anything sliding sideways, with no new
/// class for each.
/// </summary>
public class PingPongPath : MovementPath
{
    [Tooltip("Which way it travels first. (0, 1) = up, (0, -1) = down, (1, 0) = right. " +
             "Only the direction counts - the length is set below.")]
    [SerializeField] private Vector2 direction = Vector2.down;

    [Tooltip("How far it goes before turning back, in units.")]
    [Min(0f)]
    [SerializeField] private float distance = 3f;

    [Tooltip("Travel speed in units per second.")]
    [SerializeField] private float speed = 2f;

    public override Vector2 GetOffset(float time)
    {
        // PingPong walks 0 -> distance -> 0 forever, which is exactly "there and back".
        return direction.normalized * Mathf.PingPong(time * speed, distance);
    }
}
