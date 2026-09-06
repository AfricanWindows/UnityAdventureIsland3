using Game.Core;

/// <summary>
/// MODEL contract of the power bar - Adventure Island's vitality meter.
///
/// It adds nothing to IClampedCounter, and that is deliberate. Power, health and lives are
/// the same arithmetic, so the RULES are inherited rather than retyped; what this interface
/// contributes is a NAME, so that a controller asking for the power can never be handed the
/// health, and the DI container can register both.
/// </summary>
public interface IPowerModel : IClampedCounter
{
}
