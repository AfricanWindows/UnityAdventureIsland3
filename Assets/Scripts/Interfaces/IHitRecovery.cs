/// <summary>
/// The short recovery window after a hit. Whoever hurts the player starts it,
/// whoever draws it reads it - nobody depends on the concrete HitInvincibility
/// (Dependency Inversion). A separate interface from IInvincible on purpose:
/// several components on the player are IInvincible, only one is the hit recovery.
/// </summary>
public interface IHitRecovery
{
    /// <summary>Opens (or restarts) the recovery window.</summary>
    void Begin();

    /// <summary>True while the recovery window is open.</summary>
    bool IsInvincible { get; }
}
