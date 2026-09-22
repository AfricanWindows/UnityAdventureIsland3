/// <summary>
/// The short recovery window after a hit. Whoever hurts the player starts it,
/// whoever draws it reads it - nobody depends on the concrete HitInvincibility
/// (Dependency Inversion).
///
/// It IS an IInvincible, so PlayerDeath, PlayerHurt and MountHitAbsorber keep honouring it
/// through their IInvincible[] without knowing it exists. It is still its own type on
/// purpose: several components on the player are IInvincible (the fairy, dying), and only
/// ONE of them is the window a hit may open - asking for IHitRecovery finds exactly that one
/// (Interface Segregation).
/// </summary>
public interface IHitRecovery : IInvincible
{
    /// <summary>Opens (or restarts) the recovery window.</summary>
    void Begin();
}
