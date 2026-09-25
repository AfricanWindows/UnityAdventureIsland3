/// <summary>
/// ROLE: "The player died - do your part" (lose the weapon, the fairy, the animal, a life).
/// PATTERNS: Observer-style - PlayerDeath notifies every handler on the player.
/// SOLID: O - a new death rule is a new small component, PlayerDeath is not edited.
///
/// "The player has just died and is back at the start of the level - do your part."
///
/// The same shape as ILevelStartHandler, on purpose: PlayerDeath finds every component on
/// the player that implements this and calls it, exactly as LevelFlowController does with
/// ILevelStartHandler. One idea for both moments.
///
/// It replaced a STATIC event. A static event is a global: anything anywhere could listen,
/// the listeners were invisible from PlayerDeath, and every listener had to remember to
/// unsubscribe or be called after it was destroyed. Here there is nothing to subscribe, so
/// there is nothing to forget - and a new consequence of dying (losing the weapon, the fairy,
/// the animal, a life) is one more component with this interface, and PlayerDeath is never
/// edited (Open/Closed, Dependency Inversion).
/// </summary>
public interface IPlayerDeathHandler
{
    void OnPlayerDied();
}
