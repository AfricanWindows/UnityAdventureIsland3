using System;

/// <summary>
/// ROLE: The game's news: LevelEntered and GameCompleted.
/// PATTERNS: Observer - the events; DI - registered by GameInstaller.
/// SOLID: I - a listener cannot switch levels.
///
/// The game's course, as NEWS: a level was entered, the last level was finished.
///
/// Split from ILevelFlow because the two have different clients. The Level Complete panel
/// and the projectile pools only LISTEN - they must not be able to switch levels or restart
/// the game - and the door and the restart button only COMMAND (Interface Segregation).
/// LevelFlowController implements both; GameInstaller publishes it under both names.
/// </summary>
public interface ILevelEvents
{
    /// <summary>Raised when the last level has been finished.</summary>
    event Action GameCompleted;

    /// <summary>
    /// Raised every time a level is entered - the first one, the next one, and level one
    /// again after a restart. The projectile pools listen: nothing fired in the old level
    /// may still be flying in the new one.
    /// </summary>
    event Action LevelEntered;
}
