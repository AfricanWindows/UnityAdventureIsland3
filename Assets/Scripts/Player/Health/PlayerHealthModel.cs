using Game.Core;

/// <summary>
/// MODEL of the health feature: three hearts, never more, never fewer than zero.
///
/// It used to carry its own copy of the clamping and the events. That copy moved into
/// ClampedCounterModel once the power bar needed exactly the same thing, so the "maximum
/// 3 hearts" rule now lives in one place - the number handed to the constructor - and the
/// arithmetic lives in one class (Don't Repeat Yourself).
/// </summary>
public class PlayerHealthModel : ClampedCounterModel, IPlayerHealthModel
{
    public PlayerHealthModel(int maxHealth, int startHealth) : base(maxHealth, startHealth)
    {
    }
}
