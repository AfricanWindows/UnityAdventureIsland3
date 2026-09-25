using System;
using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: Runs the game: which level is on, where the player starts, what a restart does.
/// PATTERNS: DI - gets IPlayerProvider and IResetService, registered as ILevelFlow and ILevelEvents;
///           Observer - raises LevelEntered and GameCompleted.
/// SOLID: S - decides WHEN, not HOW; O - a new level is one more object in the list.
///
/// Runs the game: which level is switched on, where the player stands, and what a restart
/// means. It is the ONE place that knows there is more than one level.
///
/// There is no SceneManager here, on purpose. The levels are containers in the Hierarchy,
/// switched with SetActive, and "start the game over" is done by asking every IResettable
/// object to restore itself rather than by throwing the scene away. That is slower to
/// write and far easier to reason about: the reset is explicit, and objects that must
/// SURVIVE a restart - the player, the UI, this controller - simply are not reset.
///
/// It knows nothing about enemies, fruit or weapons. It asks IResetService to reset the game,
/// and the service finds whatever IResettable the scene happens to contain, so a new kind of
/// object joins the restart by implementing one interface (Open/Closed). This class decides
/// only WHEN to reset; HOW to find what to reset is not its job (Single Responsibility,
/// Dependency Inversion).
/// </summary>
[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class LevelFlowController : MonoBehaviour, ILevelFlow, ILevelEvents, IInjectable
{
    // Unity cannot serialize interfaces; the concrete type is only the Inspector slot, all
    // logic uses ILevel.
    [Tooltip("Every level, in play order. Level 1 first. All of them may stay switched on " +
             "in the editor - this controller turns off the ones that are not current.")]
    [SerializeField] private Level[] levels;

    [Tooltip("Log every switch and reset.")]
    [SerializeField] private bool verbose = true;

    private ILevel[] _levels;
    private IPlayerProvider _players;
    private IResetService _resets;
    private int _currentIndex = -1;

    /// <summary>Raised when the last level is finished.</summary>
    public event Action GameCompleted;

    /// <summary>Raised every time a level is entered. The projectile pools listen.</summary>
    public event Action LevelEntered;

    private ILevel CurrentLevel
    {
        get
        {
            if (_currentIndex < 0 || _currentIndex >= _levels.Length)
                return null;

            return _levels[_currentIndex];
        }
    }

    private Vector3 CurrentSpawnPosition
    {
        get
        {
            ILevel level = CurrentLevel;
            return level != null ? level.SpawnPosition : Vector3.zero;
        }
    }

    public void Inject(IServiceResolver container)
    {
        if (container == null)
            return;

        container.TryResolve(out _players);
        container.TryResolve(out _resets);
    }

    /// <summary>The one place the concrete Level type is read: copied once into ILevel.</summary>
    private void Awake()
    {
        int count = levels != null ? levels.Length : 0;
        _levels = new ILevel[count];

        for (int i = 0; i < count; i++)
        {
            // Compared as Level, not as ILevel: Unity's == turns an empty or missing
            // Inspector slot into a real null here, which an interface check would not.
            _levels[i] = levels[i] != null ? levels[i] : null;
        }
    }

    private void Start()
    {
        if (_levels.Length == 0)
        {
            Debug.LogError("LevelFlowController: no levels assigned.", this);
            return;
        }

        EnterLevel(0);
    }

    /// <summary>The door was opened. Next level, or the end of the game.</summary>
    public void GoToNextLevel()
    {
        int next = _currentIndex + 1;

        if (next >= _levels.Length)
        {
            if (verbose)
                Debug.Log("[Flow] Last level finished - game completed");

            if (GameCompleted != null)
                GameCompleted();

            return;
        }

        EnterLevel(next);
    }

    /// <summary>
    /// Everything from scratch. Every IResettable in the scene restores itself - beaten
    /// enemies come back, eaten fruit reappears, lives go back to three, weapons are lost -
    /// and then level one is entered as if the game had just been opened.
    ///
    /// Note what is NOT here: unfreezing the game. A Game Over screen is what set
    /// Time.timeScale to 0, and that same screen puts it back in its own ResetToStart - which
    /// the sweep below already calls. Pausing is the screen's business from beginning to end,
    /// so this class does not touch it (Single Responsibility). Doing it here as well was
    /// harmless but told a lie about who owns the pause.
    /// </summary>
    public void RestartGame()
    {
        int count = _resets != null ? _resets.ResetAll() : 0;
        if (_resets == null)
            Debug.LogError("[Flow] No IResetService - add a GameInstaller to the scene.", this);

        if (verbose)
            Debug.Log("[Flow] Restart - " + count + " object(s) reset");

        EnterLevel(0);
    }

    /// <summary>Switches the containers, then tells the player a new level has begun.</summary>
    private void EnterLevel(int index)
    {
        _currentIndex = index;

        for (int i = 0; i < _levels.Length; i++)
        {
            if (_levels[i] == null)
                continue;

            if (i == index)
                _levels[i].Activate();
            else
                _levels[i].Deactivate();
        }

        StartLevelForPlayer();

        if (LevelEntered != null)
            LevelEntered();

        if (verbose)
            Debug.Log("[Flow] Entered " + SafeName(CurrentLevel));
    }

    /// <summary>
    /// Announces the start of a level to whoever on the player cares: PlayerSpawn moves
    /// him and remembers the new respawn point, PowerController refills the bar,
    /// FruitCounterController starts counting from zero.
    ///
    /// This class names NONE of them. It asks for ILevelStartHandler and calls whatever
    /// it finds - which is exactly how the fruit counter joined without an edit here, and
    /// how the next thing that must react to a new level will (Open/Closed).
    /// </summary>
    private void StartLevelForPlayer()
    {
        GameObject player = _players != null ? _players.Player : null;

        if (player == null)
        {
            Debug.LogWarning("[Flow] No object tagged Player - cannot start the level on him.", this);
            return;
        }

        // true = include inactive, so a component on a switched-off child is told too.
        ILevelStartHandler[] handlers = player.GetComponentsInChildren<ILevelStartHandler>(true);
        Vector3 spawn = CurrentSpawnPosition;

        for (int i = 0; i < handlers.Length; i++)
            handlers[i].OnLevelStarted(spawn);
    }

    private static string SafeName(ILevel level)
    {
        return level != null ? level.DisplayName : "(no level)";
    }
}
