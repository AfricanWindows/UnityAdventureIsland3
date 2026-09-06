using System;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// CONTROLLER of the health feature.
///
/// It is the only piece that talks to Unity: it listens to what happens in the game
/// (spikes, hearts), tells the MODEL what to do, and pushes the result into the VIEW.
/// It holds no health rule of its own - "maximum 3" lives in PlayerHealthModel - and
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
public class PlayerHealthController : MonoBehaviour, IInjectable
{
    [Tooltip("Maximum hearts Mario can hold (exercise says 3)")]
    [SerializeField] private int maxHealth = 3;

    [Tooltip("Hearts Mario starts the level with")]
    [SerializeField] private int startHealth = 3;

    [Tooltip("Optional override. Normally left empty: the view arrives through injection.")]
    [SerializeField] private PlayerHealthView viewComponent;

    private IPlayerHealthModel model;
    private IPlayerHealthView view;

    /// <summary>Raised when Mario runs out of health. Static, so the Game Over screen
    /// does not need a reference to a player that does not exist yet.</summary>
    public static event Action OnPlayerHealthEmpty;

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
    }

    private void OnEnable()
    {
        // PlayerDeath already decides WHEN Mario is hit (it checks the star invincibility
        // and respawns him). Here we only turn that into "-1 heart".
        PlayerDeath.OnPlayerDied += LoseHealth;

        model.Changed += UpdateView;
        model.Empty += HandleHealthEmpty;
    }

    private void OnDisable()
    {
        PlayerDeath.OnPlayerDied -= LoseHealth;

        model.Changed -= UpdateView;
        model.Empty -= HandleHealthEmpty;
    }

    private void Start()
    {
        // First paint. By now every Awake has run, so the view exists if it is ever going to.
        UpdateView();
    }

    /// <summary>Entry point used by HealthPowerUp when a heart is collected.</summary>
    public bool AddHealth(int amount)
    {
        return model.Add(amount);
    }

    public void LoseHealth()
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
        if (OnPlayerHealthEmpty != null)
            OnPlayerHealthEmpty();
    }
}
