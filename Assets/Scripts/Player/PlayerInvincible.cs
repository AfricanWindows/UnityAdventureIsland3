using UnityEngine;

/// <summary>
/// The player's invincibility STATE and nothing else.
/// The countdown lives in TimedPlayerEffect, the red tint lives in TimedEffectView.
/// RequireComponent makes Unity add that view automatically, so the visual
/// can never be forgotten in the scene.
/// </summary>
[RequireComponent(typeof(TimedEffectView))]
public class PlayerInvincible : TimedPlayerEffect, IInvincible
{
    public bool IsInvincible
    {
        get { return IsActive; }
    }

    /// <summary>Entry point used by StarPowerUp.</summary>
    public void ActivateInvincibility()
    {
        Activate();
    }
}
