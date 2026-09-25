using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: Shows LEVEL COMPLETE after the last level.
/// PATTERNS: Template Method - fills Subscribe; Observer - listens to ILevelEvents.GameCompleted; DI.
///
/// Shows the LEVEL COMPLETE screen - but only when the LAST level is finished.
///
/// It used to listen to the door directly, which meant it fired at the end of every level
/// and froze the game instead of letting the next one start. Now it listens to
/// ILevelEvents.GameCompleted, so the flow controller decides what "finished" means and this
/// class only answers "when" (Single Responsibility). Everything else a screen does - the
/// panel, the freeze, closing itself on a restart - is EndScreenController's.
///
/// It gets ILevelEvents and not ILevelFlow: a screen may hear that the game ended, but it
/// has no business switching levels (Interface Segregation).
/// </summary>
public class LevelCompleteController : EndScreenController
{
    private ILevelEvents levelEvents;

    protected override void Subscribe(IServiceResolver container)
    {
        if (!container.TryResolve(out levelEvents))
        {
            Debug.LogError("LevelCompleteController: no ILevelEvents - is there a " +
                           "LevelFlowController in the scene? Level Complete will never show.", this);
            return;
        }

        levelEvents.GameCompleted += Show;
    }

    protected override void Unsubscribe()
    {
        if (levelEvents != null)
            levelEvents.GameCompleted -= Show;
    }
}
