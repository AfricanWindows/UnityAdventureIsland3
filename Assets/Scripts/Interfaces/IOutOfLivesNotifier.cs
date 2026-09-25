using System;

/// <summary>
/// ROLE: Announces "no lives left" (OutOfLives) - the Game Over screen listens.
/// PATTERNS: Observer - the event (it replaced a static event).
/// SOLID: D - the screen never names PlayerHealthController.
///
/// "The player has no lives left." The one thing the Game Over screen needs to hear.
///
/// It replaced a STATIC event on PlayerHealthController. The screen now reaches the player
/// through the IPlayerProvider it is injected with and listens to this, so it depends on
/// one small interface and not on a global that any class could raise or forget to leave
/// (Dependency Inversion, Interface Segregation).
/// </summary>
public interface IOutOfLivesNotifier
{
    event Action OutOfLives;
}
