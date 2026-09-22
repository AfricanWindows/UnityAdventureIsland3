/// <summary>
/// An obstacle in the level (stone, campfire) that riding into smashes.
/// A separate abstraction from IForceKillable on purpose: enemies can be force-killed too,
/// but riding into an enemy must not kill it - only obstacles are smashed
/// (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IObstacle : IForceKillable
{
}
