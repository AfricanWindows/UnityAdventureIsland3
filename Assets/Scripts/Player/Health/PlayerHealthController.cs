using System;
using UnityEngine;

/// <summary>
/// CONTROLLER of the health feature (exercise item 2).
///
/// It is the only piece that talks to Unity: it listens to what happens in the game
/// (spikes, hearts), tells the MODEL what to do, and pushes the result into the VIEW.
/// It holds no health rule of its own - "maximum 3" lives in PlayerHealthModel - and
/// it draws nothing itself.
///
/// It depends on the IPlayerHealthModel and IPlayerHealthView interfaces, not on the
/// concrete classes.
/// </summary>
[DisallowMultipleComponent]
public class PlayerHealthController : MonoBehaviour
{
    [Tooltip("Maximum hearts Mario can hold (exercise says 3)")]
    [SerializeField] private int maxHealth = 3;

    [Tooltip("Hearts Mario starts the level with")]
    [SerializeField] private int startHealth = 3;

    [Tooltip("Optional. Leave empty and the controller finds the view in the scene.")]
    [SerializeField] private PlayerHealthView viewComponent;

    private IPlayerHealthModel model;
    private IPlayerHealthView view;

    /// <summary>Raised when Mario runs out of health. Static, so the Game Over screen
    /// does not need a reference to a player that does not exist yet.</summary>
    public static event Action OnPlayerHealthEmpty;

    private void Awake()
    {
        model = new PlayerHealthModel(maxHealth, startHealth);
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

    [Obsolete]
    private void Start()
    {
        // Composition root of this triad. Mario is created by the Level Creator, so the
        // UI label cannot be dragged into the prefab - it is resolved once, here, and
        // never searched for again.
        view = viewComponent;

        if (view == null)
            view = FindFirstObjectByType<PlayerHealthView>();

        if (view == null)
            Debug.LogWarning("PlayerHealthController: no PlayerHealthView in the scene, health will not be shown", this);

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
