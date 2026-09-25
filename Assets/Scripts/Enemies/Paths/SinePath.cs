using UnityEngine;

/// <summary>
/// ROLE: Route straight ahead while bobbing up and down - the bird.
/// PATTERNS: Strategy - a concrete MovementPath.
///
/// Straight ahead at a steady speed while bobbing up and down - the bird.
///
/// Two independent movements added together: the horizontal one grows with time, the vertical
/// one is a sine wave around the height it was placed at. Keeping them separate is what makes
/// the three numbers below mean exactly one thing each.
/// </summary>
public class SinePath : MovementPath
{
    [Tooltip("Which way it flies. -1 = left, 1 = right.")]
    [SerializeField] private float direction = -1f;

    [Tooltip("Forward speed in units per second.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("How far above and below its start height it swings, in units.")]
    [SerializeField] private float amplitude = 1f;

    [Tooltip("Full up-and-down waves per second. Higher = more nervous flapping.")]
    [SerializeField] private float frequency = 0.5f;

    public override Vector2 GetOffset(float time)
    {
        float forward = direction >= 0f ? 1f : -1f;

        float x = forward * speed * time;

        // One full circle is 2 * PI, so multiplying by it turns "waves per second" into the
        // angle Mathf.Sin expects.
        float y = amplitude * Mathf.Sin(time * frequency * 2f * Mathf.PI);

        return new Vector2(x, y);
    }
}
