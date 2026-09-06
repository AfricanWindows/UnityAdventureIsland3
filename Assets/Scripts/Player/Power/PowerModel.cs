using Game.Core;

/// <summary>
/// MODEL of the power bar.
///
/// Empty on purpose: every rule it needs - clamp to Max, never below zero, announce Empty
/// exactly once - is already written in ClampedCounterModel, and duplicating them here to
/// look busy is how the two copies eventually drift apart.
///
/// The class exists to give the power its own type, which is what lets it be registered,
/// injected and mocked separately from the health.
/// </summary>
public class PowerModel : ClampedCounterModel, IPowerModel
{
    public PowerModel(int maxPower, int startPower) : base(maxPower, startPower)
    {
    }
}
