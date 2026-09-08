using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Shows the LEVEL COMPLETE screen - but only when the LAST level is finished.
///
/// It used to listen to the door directly, which meant it fired at the end of every level
/// and froze the game instead of letting the next one start. Now it listens to
/// ILevelFlow.GameCompleted, so the flow controller decides what "finished" means and this
/// class only draws (Single Responsibility).
///
/// Its RESTART button is the shared RestartGameButton component, exactly like the Game
/// Over popup - there is no button code here. Closing the panel is IResettable, so it
/// happens as part of the same restart that puts everything else back.
/// </summary>
public class LevelCompleteController : MonoBehaviour, IInjectable, IResettable
{
    [Tooltip("Panel with the LEVEL COMPLETE text and the RESTART button. Hidden while playing.")]
    [SerializeField] private GameObject levelCompletePanel;

    [Tooltip("Freeze the game while the panel is up.")]
    [SerializeField] private bool freezeWhileShown = true;

    private ILevelFlow flow;

    public void Inject(IServiceContainer container)
    {
        if (container == null)
            return;

        container.TryResolve(out flow);

        // Subscribed here rather than in OnEnable because injection happens before Awake,
        // and the flow could in principle finish the game on the very first frame.
        if (flow != null)
            flow.GameCompleted += OnGameCompleted;
    }

    private void OnDestroy()
    {
        if (flow != null)
            flow.GameCompleted -= OnGameCompleted;

        Time.timeScale = 1f;
    }

    private void Start()
    {
        ResetToStart();
    }

    private void OnGameCompleted()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (freezeWhileShown)
            Time.timeScale = 0f;
    }

    /// <summary>A new game: no panel, and time running again.</summary>
    public void ResetToStart()
    {
        Time.timeScale = 1f;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
}
