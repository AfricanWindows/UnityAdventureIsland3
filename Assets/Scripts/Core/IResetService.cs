namespace Game.Core
{
    /// <summary>
    /// ROLE: "Put the whole game back to its start" - the restart without reloading the scene.
    /// PATTERNS: DI - a service; the level flow asks it instead of searching the scene.
    /// SOLID: S, D - the flow decides WHEN to reset, the service knows HOW.
    ///
    /// "Put the whole game back to its starting state."
    /// The level flow asks for this instead of searching the scene itself
    /// (Single Responsibility, Dependency Inversion).
    /// </summary>
    public interface IResetService
    {
        /// <summary>Calls ResetToStart on every IResettable. Returns how many were reset.</summary>
        int ResetAll();
    }
}
