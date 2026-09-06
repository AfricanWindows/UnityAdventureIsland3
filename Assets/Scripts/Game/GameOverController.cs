using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Listens for "no health left", shows the Game Over screen and restarts the level.
/// It counts nothing itself - that is the health MODEL's job.
///
/// It subscribes to a STATIC event instead of holding a reference to Mario, because
/// Mario is created by the Level Creator and does not exist when this object wakes up.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Tooltip("Panel with the GAME OVER text. Hidden while playing.")]
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private float restartDelay = 2f;

    private bool isGameOver = false;

    private void OnEnable()
    {
        PlayerHealthController.OnPlayerHealthEmpty += OnHealthEmpty;
    }

    private void OnDisable()
    {
        PlayerHealthController.OnPlayerHealthEmpty -= OnHealthEmpty;

        // The coroutine may be killed mid freeze (scene load, object destroyed).
        // timeScale is global, so it is always restored here.
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnHealthEmpty()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Freeze the game so the player cannot keep moving during the screen.
        Time.timeScale = 0f;

        // Realtime, because timeScale is 0.
        yield return new WaitForSecondsRealtime(restartDelay);

        Time.timeScale = 1f;
        RestartLevel();
    }

    /// <summary>
    /// Reloading the scene is what "reset every object of the level" means:
    /// coins, hearts, enemies and Mario all come back in their starting state.
    /// </summary>
    private void RestartLevel()
    {
        Scene current = SceneManager.GetActiveScene();

        if (current.buildIndex < 0)
        {
            Debug.LogError("Scene '" + current.name + "' is not in Build Settings. " +
                           "Open File > Build Profiles (Build Settings) and add it, otherwise it cannot be reloaded.");
            return;
        }

        SceneManager.LoadScene(current.buildIndex);
    }
}
