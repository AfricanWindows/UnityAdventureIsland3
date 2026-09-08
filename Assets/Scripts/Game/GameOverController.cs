using Game.Core;
using UnityEngine;

/// <summary>
/// Shows the GAME OVER popup when the last life is gone.
///
/// It reloads NOTHING. The old version called SceneManager.LoadScene, which resets the
/// world by throwing it away; that is forbidden here, and it was also the wrong tool - it
/// would have destroyed the UI and this controller along with the level.
///
/// It does not own the RESTART button either: that is RestartGameButton, one component
/// shared with the Level Complete screen. This class listens for "health empty", shows a
/// panel, and hides it again when the game restarts - nothing else (Single Responsibility).
///
/// Hiding happens through IResettable, the same call that puts the enemies and the fruit
/// back, so nobody has to remember to close the popup: it closes itself as part of the
/// restart everything else already takes part in.
/// </summary>
public class GameOverController : MonoBehaviour, IResettable
{
    [Tooltip("Popup with the GAME OVER text and the RESTART button. Hidden while playing.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Freeze the game while the popup is up.")]
    [SerializeField] private bool freezeWhileShown = true;

    private bool isGameOver;

    private void OnEnable()
    {
        PlayerHealthController.OnPlayerHealthEmpty += OnHealthEmpty;
    }

    private void OnDisable()
    {
        PlayerHealthController.OnPlayerHealthEmpty -= OnHealthEmpty;

        // timeScale is global and survives everything. Leaving it at 0 here would freeze
        // the game forever, so it is always restored.
        Time.timeScale = 1f;
    }

    private void Start()
    {
        ResetToStart();
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

    /// <summary>A new game: no popup, and time running again.</summary>
    public void ResetToStart()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}
