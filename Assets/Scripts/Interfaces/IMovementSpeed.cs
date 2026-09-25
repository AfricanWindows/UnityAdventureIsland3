/// <summary>
/// ROLE: "How fast am I walking on my own?" - read by the animator view.
/// PATTERNS: none - a state interface.
/// SOLID: I, D - the view never names PlayerMovement.
///
/// "How fast am I walking on my own?" The only thing an animation view needs from movement.
/// Keeps views independent of the concrete movement class (Dependency Inversion, Interface Segregation).
/// </summary>
public interface IMovementSpeed
{
    float OwnSpeedX { get; }
}
