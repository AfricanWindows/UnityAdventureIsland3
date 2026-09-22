namespace Game.Core
{
    /// <summary>
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
