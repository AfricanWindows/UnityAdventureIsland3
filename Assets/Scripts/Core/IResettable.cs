namespace Game.Core
{
    /// <summary>
    /// "Put yourself back the way you started."
    ///
    /// This is the project's answer to "you may not reload the scene". A scene reload is a
    /// blunt instrument that resets everything by throwing it away; this resets everything
    /// by ASKING every object that has state to restore it. Each object knows what its own
    /// starting state is - an enemy knows its health, a fruit knows it was uneaten, a
    /// weapon knows whether it began locked - and nobody else has to.
    ///
    /// One method, so implementing it costs nothing (Interface Segregation), and the
    /// service that calls it never learns what an enemy or a fruit is: a new resettable
    /// object is picked up automatically and the restart code is never edited (Open/Closed,
    /// Dependency Inversion).
    /// </summary>
    public interface IResettable
    {
        void ResetToStart();
    }
}
