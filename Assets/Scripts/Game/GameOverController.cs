using Game.Core.DI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the GAME OVER popup when the last life is gone, and restarts the game when the
/// player presses the button.
///
/// It reloads NOTHING. The old version called SceneManager.LoadScene, which resets the
/// world by throwing it away; that is forbidden here, and it was also the wrong tool - it
/// would have destroyed the UI and this controller along with the level. Restarting is now
/// a message to ILevelFlow, which asks every object to restore itself.
///
/// This class counts nothing and decides nothing about lives: it listens for "health
/// empty", shows a panel, and forwards a button press (Single Responsibility).
/// </summary>
public class GameOverController : MonoBehaviour, IInjectable
{
    [Tooltip("Popup with the GAME OVER text and the RESTART button. Hidden while playing.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("The RESTART button inside the popup. Its onClick is wired up in code, so " +
             "there is nothing to drag in the Inspector.")]
    [SerializeField] private Button restartButton;

    [Tooltip("Freeze the game while the popup is up.")]
    [SerializeField] private bool freezeWhileShown = true;

    private ILevelFlow flow;
    private bool isGameOver;

    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out flow);
    }

    private void OnEnable()
    {
        PlayerHealthController.OnPlayerHealthEmpty += OnHealthEmpty;

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
    }

    private void OnDisable()
    {
        PlayerHealthController.OnPlayerHealthEmpty -= OnHealthEmpty;

        if (restartButton != null)
            restartButton.onClick.RemoveListener(Restart);

        // timeScale is global and survives everything. Leaving it at 0 here would freeze
        // the game forever, so it is always restored.
        Time.timeScale = 1f;
    }

    private void Start()
    {
        Hide();
    }

    private void OnHealthEmpty()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (freezeWhileShown)
            Time.timeScale = 0f;

        Debug.Log("[GameOver] No lives left");
    }

    /// <summary>Wired to the RESTART button. Public so the button can also call it directly.</summary>
    public void Restart()
    {
        Hide();

        if (flow != null)
            flow.RestartGame();
        else
            Debug.LogError("[GameOver] No ILevelFlow - cannot restart. Is there a " +
                           "LevelFlowController in the scene?", this);
    }

    private void Hide()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}
