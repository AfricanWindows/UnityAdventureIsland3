using UnityEngine;

/// <summary>
/// Exercise item 1: the lightning makes Mario 50% faster for 5 seconds.
///
/// The countdown is NOT written here - it lives in TimedPlayerEffect, which runs a
/// coroutine (WaitForSeconds). This class only says what "active" means for the speed,
/// and it asks PlayerMovement for a multiplier instead of writing into its field.
/// Picking up a second lightning restarts the full 5 seconds instead of stacking timers.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerSpeedBoost : TimedPlayerEffect
{
    [Tooltip("0.5 means +50% of the normal speed")]
    [SerializeField] private float speedBonus = 0.5f;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();

        // Subscribing to our own event, so there is nothing to unsubscribe from later.
        OnActiveChanged += ApplySpeed;
    }

    private void ApplySpeed(bool isActive)
    {
        if (movement == null)
            return;

        movement.SetSpeedMultiplier(isActive ? 1f + speedBonus : 1f);
    }

    /// <summary>Entry point used by SpeedPowerUp.</summary>
    public void ActivateSpeedBoost()
    {
        Activate();
    }
}
