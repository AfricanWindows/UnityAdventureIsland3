using Game.Core;

/// <summary>
/// MODEL contract of the health feature. The controller talks to this interface, never to
/// the concrete class.
///
/// Like IPowerModel it adds nothing to IClampedCounter: hearts and power segments obey the
/// same arithmetic. The separate name is what keeps the two apart at the injection point.
/// </summary>
public interface IPlayerHealthModel : IClampedCounter
{
}
