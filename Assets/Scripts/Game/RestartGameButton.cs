using Game.Core.DI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on any UI button and pressing it starts the game over.
///
/// It exists so that "restart" is written ONCE. The Game Over popup needed it first, and
/// the Level Complete screen needs the same thing; teaching each controller to hold a
/// Button field and wire its own listener would have been the same ten lines twice, and
/// the third screen would have made it three (Don't Repeat Yourself).
///
/// It knows nothing about panels, lives or levels - only ILevelFlow, which it receives
/// from the composition root (Dependency Inversion). Nothing to drag in the Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class RestartGameButton : MonoBehaviour, IInjectable
{
    private ILevelFlow flow;
    private Button button;

    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out flow);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(Restart);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Restart);
    }

    private void Restart()
    {
        if (flow != null)
            flow.RestartGame();
        else
            Debug.LogError("RestartGameButton: no ILevelFlow - is there a " +
                           "LevelFlowController in the scene?", this);
    }
}
