using System;
using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: Controller of the lives: one less on death, one more from fruit, next life or game over.
/// PATTERNS: MVC (Controller); DI - the view in Inject; Observer - listens to the model,
///           raises NextLifeStarted and OutOfLives.
/// SOLID: D - the model and the view are interfaces.
///
/// CONTROLLER of the health feature.
///
/// It is the only piece that talks to Unity: it listens to what happens in the game
/// (the player dying), tells the MODEL what to do, and pushes the result into the VIEW.
/// It holds no health rule of its own - the ceiling lives in PlayerHealthModel - and
/// it draws nothing itself.
///
/// It depends on the IPlayerHealthModel and IPlayerHealthView interfaces, not on the
/// concrete classes - and, since the DI pass, it no longer FINDS the view either. The
/// old FindFirstObjectByType call was a Service Locator: a hidden dependency that this
/// class reached out and grabbed, which meant it could not be tested, could not be given
/// a different view, and failed at runtime rather than at wiring time. The view is now
/// handed in by GameInstaller (Dependency Inversion).
///
/// It is also the one place that knows what a death MEANT, because only it knows how many
/// lives are left: the last one is gone (OutOfLives - the Game Over screen listens), or the
/// player plays on (NextLifeStarted - the power bar listens). Each outcome is its own small
/// interface, so every listener sees only the event it uses (Interface Segregation), and
/// neither listener is named here (Dependency Inversion).
/// </summary>
[DisallowMultipleComponent]
public class PlayerHealthController : MonoBehaviour, IInjectable, IResettable, IExtraLife, IPlayerDeathHandler, IOutOfLivesNotifier, INextLifeNotifier
{
    [Tooltip("Most lives the player can hold. Keep it ABOVE Start Health: an extra life for " +
             "twenty fruit that does not fit under the ceiling is lost.")]
    [SerializeField] private int maxHealth = 9;

    [Tooltip("Lives at the start of the game. The assignment says 3.")]
    [SerializeField] private int startHealth = 3;

    private IPlayerHealthModel model;
    private IPlayerHealthView view;

    /// <summary>Raised when the last life is gone. The Game Over screen listens.</summary>
    public event Action OutOfLives;

    /// <summary>Raised when a life was lost but others are left. The power bar listens.</summary>
    public event Action NextLifeStarted;

    /// <summary>
    /// Called by GameInstaller before Awake. The view arrives as IPlayerHealthView only -
    /// the controller never names the concrete view (Dependency Inversion).
    /// </summary>
    public void Inject(IServiceResolver container)
    {
        IPlayerHealthView injectedView;
        if (container != null && container.TryResolve(out injectedView))
            view = injectedView;
    }

    private void Awake()
    {
        model = new PlayerHealthModel(maxHealth, startHealth);

        if (view == null)
            Debug.LogWarning("PlayerHealthController: no IPlayerHealthView was injected - " +
                             "health will not be shown.", this);

        if (maxHealth <= startHealth)
            Debug.LogWarning("PlayerHealthController: Max Health (" + maxHealth + ") is not above " +
                             "Start Health (" + startHealth + ") - a full player gains nothing " +
                             "from twenty fruit. Raise Max Health.", this);
    }

    private void OnEnable()
    {
        // -= before += : the same defensive pair the power bar uses, so switching the player
        // object off and on again can never leave two subscriptions drawing the label twice.
        model.Changed -= UpdateView;
        model.Changed += UpdateView;
        model.Empty -= HandleHealthEmpty;
        model.Empty += HandleHealthEmpty;
    }

    private void OnDisable()
    {
        model.Changed -= UpdateView;
        model.Empty -= HandleHealthEmpty;
    }

    private void Start()
    {
        // First paint. By now every Awake has run, so the view exists if it is ever going to.
        UpdateView();
    }

    /// <summary>Three lives again. Only the whole-game restart calls this.</summary>
    public void ResetToStart()
    {
        if (model == null)
            return;

        model.Reset(startHealth);
    }

    /// <summary>Twenty fruit eaten: one life more, never past Max Health.</summary>
    public void GainLife()
    {
        if (model != null)
            model.Add(1);
    }

    /// <summary>
    /// Called by PlayerDeath once he is back at the start. PlayerDeath already decided WHEN
    /// he dies (the fairy, the recovery window); here that becomes "one life less" - and one
    /// of the two endings. The last life: the model's Empty has just raised OutOfLives.
    /// Any other: NextLifeStarted.
    /// </summary>
    public void OnPlayerDied()
    {
        model.Remove(1);

        if (model.Current > 0 && NextLifeStarted != null)
            NextLifeStarted();
    }

    private void UpdateView()
    {
        if (view != null)
            view.Render(model.Current, model.Max);
    }

    private void HandleHealthEmpty()
    {
        if (OutOfLives != null)
            OutOfLives();
    }
}
