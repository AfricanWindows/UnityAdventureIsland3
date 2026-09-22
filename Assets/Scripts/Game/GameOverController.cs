using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Shows the GAME OVER popup when the last life is gone.
///
/// Everything a screen does - the panel, the freeze, closing itself on a restart - is
/// EndScreenController's. This class answers only "when": when the player's
/// IOutOfLivesNotifier says the lives ran out (Single Responsibility).
///
/// WHERE IT HEARS IT FROM. GameInstaller injects IPlayerProvider, and this class asks the
/// player for IOutOfLivesNotifier. It used to be a static event, which any class could raise
/// and every listener had to remember to leave (Dependency Inversion).
/// </summary>
public class GameOverController : EndScreenController
{
    private IOutOfLivesNotifier lives;

    protected override void Subscribe(IServiceResolver container)
    {
        IPlayerProvider players;
        if (!container.TryResolve(out players) || players.Player == null)
        {
            Debug.LogError("GameOverController: no player found - Game Over will never show.", this);
            return;
        }

        lives = players.Player.GetComponent<IOutOfLivesNotifier>();

        if (lives == null)
        {
            Debug.LogError("GameOverController: the player has no IOutOfLivesNotifier - add a " +
                           "PlayerHealthController. Game Over will never show.", this);
            return;
        }

        lives.OutOfLives += Show;
    }

    protected override void Unsubscribe()
    {
        if (lives != null)
            lives.OutOfLives -= Show;
    }
}
