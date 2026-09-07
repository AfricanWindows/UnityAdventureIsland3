using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// CONTROLLER of the fruit counter.
///
/// It is the only piece that talks to Unity: it owns the model, pushes values into the
/// view, and turns "another twenty eaten" into a death. It holds no counting rule - the
/// lap and the leftover live in FruitCounterModel - and it draws nothing itself.
///
/// Eating twenty fruit costs a LIFE, and that goes through the project's existing death
/// path (IKillable, implemented by PlayerDeath) rather than through a second mechanism.
/// So the player respawns at the start of the level, loses a heart, drops his weapon and
/// gets a fresh power bar - all of it already written, none of it repeated here. This is
/// the same shape PowerController uses when the timer runs out.
///
/// The count is per LEVEL: ILevelStartHandler wipes it when a level begins, so the fruit
/// eaten in level one cannot kill the player in level two. That interface already existed
/// for the respawn point and the power bar - this class joined it without a single edit to
/// LevelFlowController (Open/Closed).
/// </summary>
[DisallowMultipleComponent]
public class FruitCounterController : MonoBehaviour, IInjectable, IResettable, ILevelStartHandler
{
    [Tooltip("How many fruit cost one life. The assignment says 20.")]
    [SerializeField] private int fruitsPerLife = 20;

    [Tooltip("Optional override. Normally left empty: the label arrives through injection.")]
    [SerializeField] private FruitCounterView viewComponent;

    private IFruitCounterModel model;
    private IFruitCounterView view;
    private IKillable death;

    /// <summary>Fruit eaten in this level since the last lap.</summary>
    public int Current { get { return model != null ? model.Current : 0; } }

    public void Inject(IServiceContainer container)
    {
        if (viewComponent != null)
            return;

        IFruitCounterView injectedView;
        if (container != null && container.TryResolve(out injectedView))
            view = injectedView;
    }

    private void Awake()
    {
        model = new FruitCounterModel(fruitsPerLife);
        death = GetComponent<IKillable>();

        if (viewComponent != null)
            view = viewComponent;

        if (view == null)
            Debug.LogWarning("FruitCounterController: no FruitCounterView was injected or " +
                             "assigned - the fruit count will not be shown.", this);

        if (death == null)
            Debug.LogError("FruitCounterController: no IKillable on " + gameObject.name +
                           " - eating twenty fruit will do nothing.", this);
    }

    private void OnEnable()
    {
        if (model == null)
            return;

        model.Changed -= UpdateView;
        model.Changed += UpdateView;
        model.ThresholdReached -= HandleThresholdReached;
        model.ThresholdReached += HandleThresholdReached;
    }

    private void OnDisable()
    {
        if (model == null)
            return;

        model.Changed -= UpdateView;
        model.ThresholdReached -= HandleThresholdReached;
    }

    private void Start()
    {
        // First paint. By now every Awake has run, so the view exists if it ever will.
        UpdateView();
    }

    /// <summary>Eating fruit. Called by the pickup through CountFruitPowerUp.</summary>
    public void Collect(int amount)
    {
        if (model != null)
            model.Add(amount);
    }

    /// <summary>A level began: this level's fruit is counted from zero.</summary>
    public void OnLevelStarted(Vector3 spawnPosition)
    {
        if (model != null)
            model.Reset();
    }

    /// <summary>A new game: nothing eaten yet.</summary>
    public void ResetToStart()
    {
        if (model != null)
            model.Reset();
    }

    private void UpdateView()
    {
        if (view != null)
            view.Render(model.Current, model.Threshold);
    }

    private void HandleThresholdReached()
    {
        Debug.Log("[Fruit] " + model.Threshold + " eaten - lost a life");

        if (death != null)
            death.Kill();
    }
}
