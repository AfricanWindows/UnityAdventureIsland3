/// <summary>
/// ROLE: The game's commands: finish this level, restart the game.
/// PATTERNS: DI - registered by GameInstaller.
/// SOLID: I - a commander does not see the events; D - the door never names LevelFlowController.
///
/// The game's course, as COMMANDS: finish this level, or start everything over.
///
/// Doors and the restart button talk to THIS, never to the concrete controller, so any of
/// them can be tested or replaced on its own (Dependency Inversion). What the flow ANNOUNCES
/// is a separate interface, ILevelEvents - a door has no use for the events and a pool has
/// no business finishing a level (Interface Segregation).
/// </summary>
public interface ILevelFlow
{
    /// <summary>The door was opened: switch to the next level, or finish the game.</summary>
    void GoToNextLevel();

    /// <summary>Everything from scratch: full lives, every level untouched, level one.</summary>
    void RestartGame();
}
