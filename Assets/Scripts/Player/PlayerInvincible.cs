using UnityEngine;

/// <summary>
/// The player's invincibility STATE and nothing else.
/// The countdown lives in TimedPlayerEffect, the red tint lives in TimedEffectView.
/// This class does not require or name that view: in MVC the view listens to the state,
/// the state never knows it is being drawn (Dependency Inversion).
/// </summary>
public class PlayerInvincible : TimedPlayerEffect, IInvincible, IInvincibilityEffect
{
    public bool IsInvincible
    {
        get { return IsActive; }
    }

    /// <summary>Entry point used by InvincibilityPowerUp - the fairy.</summary>
    public void ActivateInvincibility()
    {
        Activate();
    }
}
