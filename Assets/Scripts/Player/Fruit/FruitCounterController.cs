using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// CONTROLLER of the fruit counter.
///
/// It is the only piece that talks to Unity: it owns the model, pushes values into the
/// view, and turns "another twenty eaten" into an extra life. It holds no counting rule -
/// the lap and the leftover live in FruitCounterModel - and it draws nothing itself.
///
/// The life is given through IExtraLife, not through PlayerHealthController: this class
/// needs one action from the lives counter and nothing else (Interface Segregation,
/// Dependency Inversion).
///
/// The count is per LEVEL: ILevelStartHandler wipes it when a level begins, so the fruit
/// eaten in level one does not count towards a life in level two. That interface already existed
/// for the respawn point and the power bar - this class joined it without a single edit to
/// LevelFlowController (Open/Closed).
/// </summary>
[DisallowMultipleComponent]
public class FruitCounterController : MonoBehaviour, IInjectable, IResettable, ILevelStartHandler, IFruitCollector
{
    [Tooltip("How many fruit earn one extra life. The assignment says 20.")]
    [SerializeField] private int fruitsPerLife = 20;

    [Tooltip("Optional override. Normally left empty: the label arrives through injection.")]
    [SerializeField] private FruitCounterView viewComponent;

    private IFruitCounterModel model;
    private IFruitCounterView view;
    private IExtraLife lives;

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
        lives = GetComponent<IExtraLife>();

        if (viewComponent != null)
            view = viewComponent;

        if (view == null)
            Debug.LogWarning("FruitCounterController: no FruitCounterView was injected or " +
                             "assigned - the fruit count will not be shown.", this);

        if (lives == null)
            Debug.LogError("FruitCounterController: no IExtraLife on " + gameObject.name +
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
        if (lives != null)
            lives.GainLife();
    }
}
