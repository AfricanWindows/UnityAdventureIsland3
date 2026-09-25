/// <summary>
/// ROLE: "Make the player untouchable for a while" - what the fairy pickup calls.
/// PATTERNS: none - a role interface.
/// SOLID: I - one method.
///
/// Something that can make the player untouchable for a while - the fairy.
///
/// One method, so the pickup that grants it never learns how long it lasts, how it is shown
/// or that a coroutine is involved. The same shape as IExtraLife, IFruitCollector and
/// IPowerWallet: every power-up in the game reaches its target through one small interface
/// (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IInvincibilityEffect
{
    void ActivateInvincibility();
}
