/// <summary>
/// "How fast am I walking on my own?" The only thing an animation view needs from movement.
/// Keeps views independent of the concrete movement class (Dependency Inversion, Interface Segregation).
/// </summary>
public interface IMovementSpeed
{
    float OwnSpeedX { get; }
}
