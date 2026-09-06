using UnityEngine;

/// <summary>
/// Something that carries whatever stands on it: a moving floor, a lift, a conveyor.
///
/// It only reports how far it moved during the last physics step. It never touches the
/// passenger, so the platform knows nothing about Mario and Mario knows nothing about
/// the concrete platform class.
/// </summary>
public interface IRideablePlatform
{
    /// <summary>How far this platform moved in the last FixedUpdate.</summary>
    Vector2 Delta { get; }
}
