using Game.Core.DI;
using UnityEngine;

/// <summary>
/// CONTROLLER of the power bar - Adventure Island's central mechanic.
///
/// It is the only piece that talks to Unity: it owns the model, starts and stops the drain
/// clock, pushes values into the view, and turns "the bar ran out" into a death. It holds
/// no rule of its own - the clamping lives in ClampedCounterModel, the numbers live in a
/// PowerConfigSO asset - and it draws nothing itself.
///
/// Everything it touches is replaceable through an abstraction: IPowerModel, IPowerView,
/// IKillable and a config asset. Losing a life goes through the project's EXISTING death
/// path (PlayerDeath), so respawning, the lives counter and the weapon loss keep working
/// without a single line about them here.
/// </summary>
[DisallowMultipleComponent]
public class PowerController : MonoBehaviour, IInjectable
{
    [Tooltip("The asset holding Start Power, Max Power and the drain interval. Swap the " +
             "asset to change the difficulty - no code, no prefab surgery.")]
    [SerializeField] private PowerConfigSO config;

    [Tooltip("Optional override. Normally left empty: the bar arrives through injection.")]
    [SerializeField] private PowerBarView viewComponent;

    private IPowerModel model;
    private IPowerView view;
    private PowerDrainService drain;
    private IKillable death;
    private PowerStats stats;

    /// <summary>Segments right now. Read by anything that wants to show or test it.</summary>
    public int Current { get { return model != null ? model.Current : 0; } }

    /// <summary>The ceiling in force, straight from the config asset.</summary>
    public int Max { get { return model != null ? model.Max : 0; } }

    /// <summary>Called by GameInstaller before Awake. A hand-wired view still wins.</summary>
    public void Inject(IServiceContainer container)
    {
        if (viewComponent != null)
            return;

        IPowerView injectedView;
        if (container != null && container.TryResolve(out injectedView))
            view = injectedView;
    }

    private void Awake()
    {
        if (config == null)
        {
            // Disabled rather than limping on with invented numbers: a bar with a
            // guessed ceiling is worse than an obviously missing one.
            Debug.LogError("PowerController: no Power Config asset assigned on " +
                           gameObject.name + " - the bar is switched off.", this);
            enabled = false;
            return;
        }

        stats = config.Stats;

        model = new PowerModel(stats.MaxPower, stats.StartPower);
        drain = new PowerDrainService(model, stats.DrainIntervalSeconds);
        death = GetComponent<IKillable>();

        if (viewComponent != null)
            view = viewComponent;

        if (view == null)
            Debug.LogWarning("PowerController: no PowerBarView was injected or assigned - " +
                             "the bar will not be drawn.", this);

        if (death == null)
            Debug.LogError("PowerController: no IKillable on " + gameObject.name +
                           " - running out of power will do nothing.", this);
    }

    private void OnEnable()
    {
        if (model == null)
            return;

        model.Changed += UpdateView;
        model.Empty += HandleEmpty;

        drain.Start();
    }

    private void OnDisable()
    {
        if (model == null)
            return;

        // The important half. Without this the Task.Delay loop outlives Play Mode.
        drain.Stop();

        model.Changed -= UpdateView;
        model.Empty -= HandleEmpty;
    }

    private void Start()
    {
        // First paint. By now every Awake has run, so the view exists if it ever will -
        // and this is what tells the bar how many sockets to build.
        UpdateView();
    }

    /// <summary>Eating a fruit. Returns how many segments actually fitted.</summary>
    public int AddPower(int amount)
    {
        return model != null ? model.Add(amount) : 0;
    }

    private void UpdateView()
    {
        if (view != null)
            view.Render(model.Current, model.Max);
    }

    /// <summary>
    /// The bar ran out. Refill FIRST, then die.
    ///
    /// The order matters: Kill() respawns the player, and he must come back with a full
    /// bar. It also means an ignored kill - the star makes PlayerDeath refuse - still
    /// leaves a running bar instead of a player stuck at zero with a clock that can never
    /// fire Empty again.
    /// </summary>
    private void HandleEmpty()
    {
        Debug.Log("[Power] Bar empty - lost a life");

        model.Reset(stats.StartPower);

        if (death != null)
            death.Kill();
    }
}
