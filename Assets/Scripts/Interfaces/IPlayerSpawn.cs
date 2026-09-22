/// <summary>
/// "Put the player back where the current level starts."
///
/// PlayerDeath needs exactly this and nothing more: once the death animation is over, the
/// player goes back to the start. WHERE that is - and that it changes when a new level
/// begins - is not dying's business (Single Responsibility, Dependency Inversion).
/// </summary>
public interface IPlayerSpawn
{
    /// <summary>Moves the player to the current level's spawn point, standing still.</summary>
    void ReturnToSpawn();
}
