using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Shows the LEVEL COMPLETE screen - but only when the LAST level is finished.
///
/// It used to listen to the door directly, which meant it fired at the end of every level
/// and froze the game instead of letting the next one start. Now it listens to
/// ILevelFlow.GameCompleted, so the flow controller decides what "finished" means and this
/// class only draws (Single Responsibility).
/// </summary>
public class LevelCompleteController : MonoBehaviour, IInjectable
{
    [Tooltip("Panel with the LEVEL COMPLETE text. Hidden while playing.")]
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
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    private void OnGameCompleted()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (freezeWhileShown)
            Time.timeScale = 0f;
    }
}
