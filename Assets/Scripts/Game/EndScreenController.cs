using Game.Core;
using Game.Core.DI;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ROLE: Base of the end screens: shows a panel, pauses the game, closes itself on a restart.
/// PATTERNS: Template Method - subclasses fill Subscribe and Unsubscribe (WHEN to show); DI.
/// SOLID: S - the only owner of the pause (Time.timeScale); O - a third screen is a small subclass.
///
/// An end-of-game screen: a panel that appears when something announces "the game is over"
/// - no lives left, or the last level finished - freezes the game while it is up, and
/// closes itself when the game restarts.
///
/// GameOverController and LevelCompleteController used to be the same class twice, different
/// only in WHICH event opens the panel. That one difference is now the only thing a subclass
/// writes: it subscribes in Subscribe(), lets go in Unsubscribe(), and hands Show() to its
/// event. The panel, the freeze and the reset are written here once (Template Method,
/// Open/Closed - a third screen is one small subclass and no change here).
///
/// It reloads NOTHING - no SceneManager. Hiding happens through IResettable, the same call
/// that puts the enemies and the fruit back, so nobody has to remember to close the panel:
/// it closes itself as part of the restart everything else already takes part in. The
/// RESTART button is the shared RestartGameButton component, so there is no button code here.
/// </summary>
public abstract class EndScreenController : MonoBehaviour, IInjectable, IResettable
{
    // Both old names: the Game Over popup called it gameOverPanel, the Level Complete screen
    // levelCompletePanel. FormerlySerializedAs keeps the panels already wired in the scene.
    [Tooltip("The panel with the text and the RESTART button. Hidden while playing.")]
    [FormerlySerializedAs("gameOverPanel")]
    [FormerlySerializedAs("levelCompletePanel")]
    [SerializeField] private GameObject panel;

    [Tooltip("Freeze the game while the panel is up.")]
    [SerializeField] private bool freezeWhileShown = true;

    private bool isShown;

    /// <summary>
    /// Called by GameInstaller before Awake. Subscribed here rather than in OnEnable because
    /// injection happens before Awake, and the event could in principle arrive on the very
    /// first frame.
    /// </summary>
    public void Inject(IServiceResolver container)
    {
        if (container != null)
            Subscribe(container);
    }

    /// <summary>The one step each screen writes: find its event and hand it Show().</summary>
    protected abstract void Subscribe(IServiceResolver container);

    /// <summary>Lets go of whatever Subscribe() subscribed to.</summary>
    protected abstract void Unsubscribe();

    private void Start()
    {
        ResetToStart();
    }

    // timeScale is global and survives everything. Leaving it at 0 when this screen goes
    // away would freeze the game forever, so it is always restored.
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>The game is over: show the panel once, and freeze the world behind it.</summary>
    protected void Show()
    {
        if (isShown)
            return;

        isShown = true;

        if (panel != null)
            panel.SetActive(true);

        if (freezeWhileShown)
            Time.timeScale = 0f;
    }

    /// <summary>A new game: no panel, and time running again.</summary>
    public void ResetToStart()
    {
        isShown = false;
        Time.timeScale = 1f;

        if (panel != null)
            panel.SetActive(false);
    }
}
