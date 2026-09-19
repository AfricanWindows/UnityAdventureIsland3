using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Shows the GAME OVER popup when the last life is gone.
///
/// It reloads NOTHING. The old version called SceneManager.LoadScene, which resets the
/// world by throwing it away; that is forbidden here, and it was also the wrong tool - it
/// would have destroyed the UI and this controller along with the level.
///
/// It does not own the RESTART button either: that is RestartGameButton, one component
/// shared with the Level Complete screen. This class listens for "no lives left", shows a
/// panel, and hides it again when the game restarts - nothing else (Single Responsibility).
///
/// WHERE IT HEARS IT FROM. GameInstaller injects IPlayerProvider, and this class asks the
/// player for IOutOfLivesNotifier - the same way LevelCompleteController is injected with
/// ILevelFlow and listens to GameCompleted. It used to be a static event, which any class
/// could raise and every listener had to remember to leave (Dependency Inversion).
///
/// Hiding happens through IResettable, the same call that puts the enemies and the fruit
/// back, so nobody has to remember to close the popup: it closes itself as part of the
/// restart everything else already takes part in.
/// </summary>
public class GameOverController : MonoBehaviour, IInjectable, IResettable
{
    [Tooltip("Popup with the GAME OVER text and the RESTART button. Hidden while playing.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Freeze the game while the popup is up.")]
    [SerializeField] private bool freezeWhileShown = true;

    private IOutOfLivesNotifier lives;
    private bool isGameOver;

    /// <summary>Called by GameInstaller before Awake.</summary>
    public void Inject(IServiceContainer container)
    {
        IPlayerProvider players;
        if (container == null || !container.TryResolve(out players) || players.Player == null)
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

        lives.OutOfLives += OnOutOfLives;
    }

    private void OnDestroy()
    {
        if (lives != null)
            lives.OutOfLives -= OnOutOfLives;
    }

    private void OnDisable()
    {
        // timeScale is global and survives everything. Leaving it at 0 here would freeze
        // the game forever, so it is always restored.
        Time.timeScale = 1f;
    }

    private void Start()
    {
        ResetToStart();
    }

    private void OnOutOfLives()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (freezeWhileShown)
            Time.timeScale = 0f;
    }

    /// <summary>A new game: no popup, and time running again.</summary>
    public void ResetToStart()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}
