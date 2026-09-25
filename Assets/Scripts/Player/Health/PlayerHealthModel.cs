using Game.Core;

/// <summary>
/// ROLE: Model of the lives - empty, the rules are in ClampedCounterModel.
/// PATTERNS: MVC (Model).
///
/// MODEL of the lives counter: never above its ceiling, never below zero. Both numbers -
/// the lives at the start and the most he may hold - are set on PlayerHealthController and
/// handed to the constructor.
///
/// It used to carry its own copy of the clamping and the events. That copy moved into
/// ClampedCounterModel once the power bar needed exactly the same thing, so the ceiling rule
/// now lives in one place - the number handed to the constructor - and the arithmetic lives
/// in one class (Don't Repeat Yourself).
/// </summary>
public class PlayerHealthModel : ClampedCounterModel, IPlayerHealthModel
{
    public PlayerHealthModel(int maxHealth, int startHealth) : base(maxHealth, startHealth)
    {
    }
}
