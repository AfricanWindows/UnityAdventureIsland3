using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// CONTROLLER of the power bar - Adventure Island's central mechanic.
///
/// It is the only piece that talks to Unity: it owns the model, ticks the drain clock once
/// per frame, pushes values into the view, and turns "the bar ran out" into a death. It holds
/// no rule of its own - the clamping lives in ClampedCounterModel, the numbers live in a
/// PowerConfigSO asset - and it draws nothing itself.
///
/// Everything it touches is replaceable through an abstraction: IPowerModel, IPowerView,
/// IForceKillable and a config asset. Losing a life goes through the project's EXISTING death
/// path (PlayerDeath), so respawning, the lives counter and the weapon loss keep working
/// without a single line about them here.
/// </summary>
[DisallowMultipleComponent]
public class PowerController : MonoBehaviour, IInjectable, IResettable, ILevelStartHandler, IPlayerDeathHandler, IPowerWallet
{
    [Tooltip("The asset holding Start Power, Max Power and the drain interval. Swap the " +
             "asset to change the difficulty - no code, no prefab surgery.")]
    [SerializeField] private PowerConfigSO config;

    [Tooltip("Optional override. Normally left empty: the bar arrives through injection.")]
    [SerializeField] private PowerBarView viewComponent;

    private IPowerModel model;
    private IPowerView view;
    private PowerDrainService drain;
    private IForceKillable death;
    private PowerStats stats;

    /// <summary>Called by GameInstaller before Awake. A hand-wired view still wins.</summary>
    public void Inject(IServiceResolver container)
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
        death = GetComponent<IForceKillable>();

        if (viewComponent != null)
            view = viewComponent;

        if (view == null)
            Debug.LogWarning("PowerController: no PowerBarView was injected or assigned - " +
                             "the bar will not be drawn.", this);

        if (death == null)
            Debug.LogError("PowerController: no IForceKillable on " + gameObject.name +
                           " - running out of power will do nothing.", this);
    }

    private void OnEnable()
    {
        if (model == null)
            return;

        // -= before += : subscribing twice would draw the bar twice per tick and lose a
        // life twice when it empties. OnEnable runs again every time the player object is
        // switched back on, so the pair is written defensively.
        model.Changed -= UpdateView;
        model.Changed += UpdateView;
        model.Empty -= HandleEmpty;
        model.Empty += HandleEmpty;
    }

    private void OnDisable()
    {
        if (model == null)
            return;

        model.Changed -= UpdateView;
        model.Empty -= HandleEmpty;
    }

    // Only runs while this component is enabled, and Time.deltaTime is 0 while the game is
    // paused - so the bar stops draining exactly when the game stops, with no pause logic.
    private void Update()
    {
        if (drain != null)
            drain.Tick(Time.deltaTime);
    }

    private void Start()
    {
        // First paint. By now every Awake has run, so the view exists if it ever will -
        // and this is what tells the bar how many sockets to build.
        UpdateView();
    }

    /// <summary>
    /// A full bar and a fresh clock. Called when a level is entered, when the player
    /// respawns, and by the whole-game restart - the three moments the timer must not
    /// carry anything over from before.
    /// </summary>
    public void ResetToStart()
    {
        if (model == null)
            return;

        model.Reset(stats.StartPower);
        drain.Restart();

        UpdateView();
    }

    /// <summary>A level began: full bar, fresh clock. The spawn point is not our business.</summary>
    public void OnLevelStarted(Vector3 spawnPosition)
    {
        ResetToStart();
    }

    /// <summary>
    /// Any death refills the bar - an enemy, spikes, or the timer itself. Called by
    /// PlayerDeath once he is back at the start.
    /// </summary>
    public void OnPlayerDied()
    {
        ResetToStart();
    }

    /// <summary>
    /// Losing power to a hazard - the stone costs three. Returns how many segments
    /// were actually taken, which is fewer than asked when the bar was nearly empty.
    ///
    /// It goes through the model like everything else, so hitting zero this way raises
    /// the same Empty event and costs the same life as running out of time.
    /// </summary>
    public int RemovePower(int amount)
    {
        return model != null ? model.Remove(amount) : 0;
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
    /// The order matters: the death respawns the player, and he must come back with a full
    /// bar - and a death that is already under way still leaves a running bar instead of a
    /// player stuck at zero with a clock that can never fire Empty again.
    ///
    /// ForceKill, not Kill - the same door the abyss uses. Running out of power
    /// is a RULE of the game, not a blow, so no protection may refuse it. With Kill the fairy
    /// (or the recovery window after a hit) made PlayerDeath say no, and the
    /// refill above then turned an empty bar into a free full one.
    /// </summary>
    private void HandleEmpty()
    {
        model.Reset(stats.StartPower);

        if (death != null)
            death.ForceKill();
    }
}
