using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: Controller of the power bar: ticks the drain, draws the bar, an empty bar = death.
/// PATTERNS: MVC (Controller); DI - the view in Inject; Observer - listens to Changed and Empty.
/// SOLID: S - no rules, no drawing; D - every collaborator is an interface (the config is a data asset).
///
/// CONTROLLER of the power bar - Adventure Island's central mechanic.
///
/// It is the only piece that talks to Unity: it owns the model, ticks the drain clock once
/// per frame, pushes values into the view, and turns "the bar ran out" into a death. It holds
/// no rule of its own - the clamping lives in ClampedCounterModel, the numbers live in a
/// PowerConfigSO asset - and it draws nothing itself.
///
/// Everything it touches is replaceable through an abstraction: IPowerModel, IPowerDrain,
/// IPowerView, IForceKillable, INextLifeNotifier and a config asset. It CREATES the model and
/// the drain clock, which is the owner's privilege - they are private to this bar and nothing
/// else may share them - but it never names those classes again afterwards. Losing a life
/// goes through the project's EXISTING death path (PlayerDeath), so respawning, the lives
/// counter and the weapon loss keep working without a single line about them here.
///
/// WHEN THE BAR IS FULL AGAIN: only when the player stands at a start ALIVE - a level begins,
/// the game restarts, or the lives counter says a next life begins. Never at the moment of
/// death: an empty bar stays empty through the death animation, and behind the Game Over
/// screen if that was the last life.
/// </summary>
[DisallowMultipleComponent]
public class PowerController : MonoBehaviour, IInjectable, IResettable, ILevelStartHandler, IPowerWallet
{
    [Tooltip("The asset holding Start Power, Max Power and the drain interval. Swap the " +
             "asset to change the difficulty - no code, no prefab surgery.")]
    [SerializeField] private PowerConfigSO config;

    private IPowerModel model;
    private IPowerView view;
    private IPowerDrain drain;
    private IForceKillable death;
    private INextLifeNotifier lives;
    private PowerStats stats;

    /// <summary>
    /// Called by GameInstaller before Awake. The bar arrives as IPowerView only - the
    /// controller never names the concrete view (Dependency Inversion).
    /// </summary>
    public void Inject(IServiceResolver container)
    {
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
        lives = GetComponent<INextLifeNotifier>();

        if (view == null)
            Debug.LogWarning("PowerController: no IPowerView was injected - " +
                             "the bar will not be drawn.", this);

        if (death == null)
            Debug.LogError("PowerController: no IForceKillable on " + gameObject.name +
                           " - running out of power will do nothing.", this);

        if (lives == null)
            Debug.LogError("PowerController: no INextLifeNotifier on " + gameObject.name +
                           " - the bar will not refill after a death. Add a Player Health " +
                           "Controller.", this);
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

        if (lives != null)
        {
            lives.NextLifeStarted -= HandleNextLife;
            lives.NextLifeStarted += HandleNextLife;
        }
    }

    private void OnDisable()
    {
        if (model == null)
            return;

        model.Changed -= UpdateView;
        model.Empty -= HandleEmpty;

        if (lives != null)
            lives.NextLifeStarted -= HandleNextLife;
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
    /// A full bar and a fresh clock. Called when a level is entered, when a next life begins
    /// after a death, and by the whole-game restart - the three moments the timer must not
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
    /// He died, had a life to spare, and is back at the start alive: full bar, fresh clock.
    /// The last death never gets here, so the Game Over screen shows the bar as it was.
    /// </summary>
    private void HandleNextLife()
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
    /// The bar ran out: he dies. Nothing is refilled here - the bar stays empty until
    /// HandleNextLife, a new level or a restart fills it.
    ///
    /// It cannot get stuck at zero. The model announces Empty only once, so a bar that ran
    /// out while a death was already under way (ForceKill is refused then) needs that death
    /// to refill it - and it does: every death ends in a next life or in a Game Over, and the
    /// restart after a Game Over refills it too.
    ///
    /// ForceKill, not Kill - the same door the abyss uses. Running out of power is a RULE of
    /// the game, not a blow, so no protection may refuse it. With Kill the fairy (or the
    /// recovery window after a hit) would make PlayerDeath say no, and the player would walk
    /// on with an empty bar that never fires Empty again.
    /// </summary>
    private void HandleEmpty()
    {
        if (death != null)
            death.ForceKill();
    }
}
