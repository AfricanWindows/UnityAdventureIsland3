using System;

/// <summary>
/// ROLE: Announces "he lost a life and plays on" (NextLifeStarted) - the power bar refills on it.
/// PATTERNS: Observer - the event.
/// SOLID: D - the power bar never names PlayerHealthController.
///
/// "The player lost a life, has more left, and plays on." The moment he is back at the start
/// of the level, alive - which is when the power bar refills.
///
/// The twin of IOutOfLivesNotifier: every death ends in exactly one of the two, and the lives
/// counter - the only class that knows how many are left - raises whichever it is. Two small
/// interfaces and not two events on one, because each listener needs only its own: the Game
/// Over screen has no use for "play on", and the power bar none for "game over" (Interface
/// Segregation).
///
/// It exists so the power bar no longer refills as an IPlayerDeathHandler. A death handler
/// cannot tell a death the game continues after from the last one: the lives are counted by
/// ANOTHER handler, and the order handlers are called in is only the order of components in
/// the Inspector. Refilling there put a full bar behind the Game Over screen.
/// </summary>
public interface INextLifeNotifier
{
    event Action NextLifeStarted;
}
