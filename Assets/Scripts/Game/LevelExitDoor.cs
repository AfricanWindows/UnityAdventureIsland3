using Game.Core.DI;
using UnityEngine;

/// <summary>
/// The end of a level: the player touches it, the level is finished.
///
/// It only DETECTS. What happens next - switching to the next level, or showing the win
/// screen after the last one - belongs to ILevelFlow (Single Responsibility).
///
/// It used to demand a key first. That was a leftover from an earlier exercise and is not
/// in the final assignment, which says only "reach the end of the level, and the second
/// one starts at once". The key was also carried between levels, so the one found in
/// level one silently unlocked level two as well.
///
/// ILevelFlow arrives through injection, exactly like RestartGameButton's, so no door needs
/// a reference dragged in the Inspector. It used to be a static event instead - a global
/// that the flow controller had to find and subscribe to (Dependency Inversion).
/// </summary>
public class LevelExitDoor : MonoBehaviour, IInjectable
{
    [SerializeField] private string playerTag = "Player";

    private ILevelFlow flow;

    // A trigger can report the same contact more than once - a player with a body collider
    // and a foot collider enters twice - and finishing the level twice would skip a level.
    private bool completed;

    /// <summary>Called by GameInstaller, also for the doors of levels that start switched off.</summary>
    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out flow);
    }

    // Cleared when the level is switched back on, so a replayed level can be finished again.
    private void OnEnable()
    {
        completed = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (completed || col == null || !col.gameObject.CompareTag(playerTag))
            return;

        if (flow == null)
        {
            Debug.LogError("LevelExitDoor: no ILevelFlow - is there a LevelFlowController " +
                           "in the scene?", this);
            return;
        }

        completed = true;
        flow.GoToNextLevel();
    }
}
