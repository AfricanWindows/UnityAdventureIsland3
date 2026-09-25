/// <summary>
/// ROLE: The fairy's invincibility STATE (IInvincible), counted down by TimedPlayerEffect.
/// PATTERNS: MVC-style state - TimedEffectObjectView is its view and listens to it.
/// SOLID: D - it never names its view.
///
/// The player's invincibility STATE and nothing else.
/// The countdown lives in TimedPlayerEffect, the fairy flying beside him is
/// TimedEffectObjectView. This class does not require or name that view: in MVC the view
/// listens to the state, the state never knows it is being drawn (Dependency Inversion).
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
