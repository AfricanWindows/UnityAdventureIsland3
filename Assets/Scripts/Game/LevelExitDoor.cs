using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: The level's exit: touching it tells ILevelFlow the level is finished.
/// PATTERNS: Template Method - fills Affect of PlayerContactEffect; DI - gets ILevelFlow.
/// SOLID: S - it only detects.
///
/// The end of a level: the player touches it, the level is finished.
///
/// It only DETECTS. What happens next - switching to the next level, or showing the win
/// screen after the last one - belongs to ILevelFlow (Single Responsibility). And even the
/// detecting is not written here: "is this the player, is it a new touch" is
/// PlayerContactEffect's, and this class fills in the one step that is its own (Template
/// Method).
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
public class LevelExitDoor : PlayerContactEffect, IInjectable
{
    private ILevelFlow flow;

    // Once per touch is not enough for a door: after the LAST level nothing switches it off,
    // and stepping out and back in must not finish the game a second time. Cleared only when
    // the level is switched back on, so a replayed level can be finished again.
    private bool completed;

    /// <summary>Called by GameInstaller, also for the doors of levels that start switched off.</summary>
    public void Inject(IServiceResolver container)
    {
        if (container != null)
            container.TryResolve(out flow);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        completed = false;
    }

    protected override void Affect(GameObject player)
    {
        if (completed)
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
