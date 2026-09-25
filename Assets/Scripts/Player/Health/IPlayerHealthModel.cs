using Game.Core;

/// <summary>
/// ROLE: Model contract of the lives - adds only a name to IClampedCounter.
/// PATTERNS: MVC (Model contract).
/// SOLID: the name keeps the lives and the power apart: mixing them is a compile error.
///
/// MODEL contract of the health feature. The controller talks to this interface, never to
/// the concrete class.
///
/// Like IPowerModel it adds nothing to IClampedCounter: hearts and power segments obey the
/// same arithmetic. The separate name is what keeps the two apart at the injection point.
/// </summary>
public interface IPlayerHealthModel : IClampedCounter
{
}
