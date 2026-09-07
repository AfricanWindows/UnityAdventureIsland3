using System;
using UnityEngine;

/// <summary>
/// The game's course: which level is running, what happens when it is finished, and what
/// "start over" means.
///
/// Doors, the Game Over popup and the player all talk to THIS, never to the concrete
/// controller, so any of them can be tested or replaced on its own (Dependency Inversion).
/// </summary>
public interface ILevelFlow
{

    /// <summary>The door was opened: switch to the next level, or finish the game.</summary>
    void GoToNextLevel();

    /// <summary>Everything from scratch: full lives, every level untouched, level one.</summary>
    void RestartGame();

    /// <summary>Raised when the last level has been finished.</summary>
    event Action GameCompleted;
}
