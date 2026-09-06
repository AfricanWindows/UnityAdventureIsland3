/// <summary>
/// Answers one question: what am I standing on right now, if it can carry me?
///
/// Kept apart from IGroundCheck on purpose: "am I grounded" and "what carries me" are
/// two different questions, and a class should not be forced to answer one it does not
/// care about (Interface Segregation).
/// </summary>
public interface IPlatformProvider
{
    IRideablePlatform CurrentPlatform { get; }
}
