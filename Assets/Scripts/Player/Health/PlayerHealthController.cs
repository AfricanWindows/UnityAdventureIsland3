using System;
using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
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
/// </summary>
[DisallowMultipleComponent]
public class PlayerHealthController : MonoBehaviour, IInjectable, IResettable, IExtraLife, IPlayerDeathHandler, IOutOfLivesNotifier
{
    [Tooltip("Most lives the player can hold. Keep it ABOVE Start Health: an extra life for " +
             "twenty fruit that does not fit under the ceiling is lost.")]
    [SerializeField] private int maxHealth = 9;

    [Tooltip("Lives at the start of the game. The assignment says 3.")]
    [SerializeField] private int startHealth = 3;

    [Tooltip("Optional override. Normally left empty: the view arrives through injection.")]
    [SerializeField] private PlayerHealthView viewComponent;

    private IPlayerHealthModel model;
    private IPlayerHealthView view;

    /// <summary>Raised when the last life is gone. The Game Over screen listens.</summary>
    public event Action OutOfLives;

    /// <summary>
    /// Called by GameInstaller before Awake. An explicit field on this object still wins:
    /// a hand-wired reference is a deliberate decision, and injection should not overrule it.
    /// </summary>
    public void Inject(IServiceContainer container)
    {
        if (viewComponent != null)
            return;

        IPlayerHealthView injectedView;
        if (container != null && container.TryResolve(out injectedView))
            view = injectedView;
    }

    private void Awake()
    {
        model = new PlayerHealthModel(maxHealth, startHealth);

        if (viewComponent != null)
            view = viewComponent;

        if (view == null)
            Debug.LogWarning("PlayerHealthController: no health view was injected or assigned - " +
                             "health will not be shown.", this);

        if (maxHealth <= startHealth)
            Debug.LogWarning("PlayerHealthController: Max Health (" + maxHealth + ") is not above " +
                             "Start Health (" + startHealth + ") - a full player gains nothing " +
                             "from twenty fruit. Raise Max Health.", this);
    }

    private void OnEnable()
    {
        model.Changed += UpdateView;
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
    /// he dies (the fairy, the recovery window); here that only becomes "one life less".
    /// </summary>
    public void OnPlayerDied()
    {
        model.Remove(1);
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
