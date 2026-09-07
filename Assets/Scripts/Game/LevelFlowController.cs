using System;
using System.Collections.Generic;
using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Runs the game: which level is switched on, where the player stands, and what a restart
/// means. It is the ONE place that knows there is more than one level.
///
/// There is no SceneManager here, on purpose. The levels are containers in the Hierarchy,
/// switched with SetActive, and "start the game over" is done by asking every IResettable
/// object to restore itself rather than by throwing the scene away. That is slower to
/// write and far easier to reason about: the reset is explicit, and objects that must
/// SURVIVE a restart - the player, the UI, this controller - simply are not reset.
///
/// It knows nothing about enemies, fruit or weapons. It asks for IResettable and gets
/// whatever the scene happens to contain, so a new kind of object joins the restart by
/// implementing one interface (Open/Closed).
/// </summary>
[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class LevelFlowController : MonoBehaviour, ILevelFlow, IInjectable
{
    [Tooltip("Every level, in play order. Level 1 first. All of them may stay switched on " +
             "in the editor - this controller turns off the ones that are not current.")]
    [SerializeField] private Level[] levels;

    [Tooltip("Log every switch and reset.")]
    [SerializeField] private bool verbose = true;

    private IPlayerProvider _players;
    private int _currentIndex = -1;

    /// <summary>Raised when the last level is finished.</summary>
    public event Action GameCompleted;

    public Level CurrentLevel
    {
        get
        {
            if (levels == null || _currentIndex < 0 || _currentIndex >= levels.Length)
                return null;

            return levels[_currentIndex];
        }
    }

    public Vector3 CurrentSpawnPosition
    {
        get
        {
            Level level = CurrentLevel;
            return level != null ? level.SpawnPosition : Vector3.zero;
        }
    }

    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out _players);
    }

    private void OnEnable()
    {
        LevelExitDoor.OnLevelCompleted += GoToNextLevel;
    }

    private void OnDisable()
    {
        LevelExitDoor.OnLevelCompleted -= GoToNextLevel;
    }

    private void Start()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelFlowController: no levels assigned.", this);
            return;
        }

        EnterLevel(0);
    }

    /// <summary>

    /// <summary>The door was opened. Next level, or the end of the game.</summary>
    public void GoToNextLevel()
    {
        if (levels == null)
            return;

        int next = _currentIndex + 1;

        if (next >= levels.Length)
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
    /// </summary>
    public void RestartGame()
    {
        // A Game Over screen freezes the game; a restart that forgot this would hand back
        // a frozen world.
        Time.timeScale = 1f;

        int count = ResetEverything();

        if (verbose)
            Debug.Log("[Flow] Restart - " + count + " object(s) reset");

        EnterLevel(0);
    }

    /// <summary>Switches the containers, then tells the player a new level has begun.</summary>
    private void EnterLevel(int index)
    {
        _currentIndex = index;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null)
                continue;

            if (i == index)
                levels[i].Activate();
            else
                levels[i].Deactivate();
        }

        StartLevelForPlayer();

        if (verbose)
            Debug.Log("[Flow] Entered " + SafeName(CurrentLevel));
    }

    /// <summary>
    /// <summary>
    /// Announces the start of a level to whoever on the player cares: PlayerDeath moves
    /// him and remembers the new respawn point, PowerController refills the bar.
    ///
    /// This class names NEITHER of them. It asks for ILevelStartHandler and calls
    /// whatever it finds, so a third thing that must react to a new level - a checkpoint
    /// marker, an ammo refill - joins by implementing one method (Open/Closed).
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

    /// <summary>
    /// Asks every resettable object in the scene to restore itself.
    ///
    /// Inactive objects are included: level two is switched off during level one, and its
    /// enemies still have to be reset. This is the one broad search in the game flow, and
    /// it runs only on a restart - never per frame.
    /// </summary>
    private int ResetEverything()
    {
        MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
        List<IResettable> targets = new List<IResettable>();

        for (int i = 0; i < all.Length; i++)
        {
            IResettable resettable = all[i] as IResettable;

            if (resettable != null)
                targets.Add(resettable);
        }

        // Collected first, then called: a ResetToStart that switches an object back on
        // must not disturb the array we are walking.
        for (int i = 0; i < targets.Count; i++)
            targets[i].ResetToStart();

        return targets.Count;
    }

    private static string SafeName(Level level)
    {
        return level != null ? level.DisplayName : "(no level)";
    }
}
