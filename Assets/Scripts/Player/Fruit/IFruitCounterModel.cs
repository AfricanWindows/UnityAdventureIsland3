using System;

/// <summary>
/// MODEL contract of the fruit counter.
///
/// It counts UP to a threshold and wraps, which is why it is not an IClampedCounter: that
/// one has a ceiling it refuses to cross, this one crosses on purpose and starts again.
/// Health, power and lives clamp; fruit laps.
/// </summary>
public interface IFruitCounterModel
{
    /// <summary>Fruit eaten since the last lap. Always between 0 and Threshold - 1.</summary>
    int Current { get; }

    /// <summary>How many fruit make one lap. 20 in the assignment.</summary>
    int Threshold { get; }

    /// <summary>Raised on every change, so the label redraws.</summary>
    event Action Changed;

    /// <summary>Raised once per completed lap. Eating 40 in one go raises it twice.</summary>
    event Action ThresholdReached;

    /// <summary>Eating fruit.</summary>
    void Add(int amount);

    /// <summary>Back to zero - a new level, or a new game.</summary>
    void Reset();
}
