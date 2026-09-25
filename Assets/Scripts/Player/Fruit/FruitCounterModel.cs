using System;

/// <summary>
/// ROLE: Model of the fruit counter: counts up to 20, then starts a new lap.
/// PATTERNS: MVC (Model) - plain C#; Observer - raises Changed and ThresholdReached.
/// SOLID: S - it does not know that a lap gives a life.
///
/// MODEL of the fruit counter. It owns the DATA and the ONE rule: every Threshold fruit
/// completes a lap, and the count starts again from what is left over.
///
/// It does NOT know that a lap earns a life. That is the consequence, not the rule, and
/// it belongs to the controller - which is exactly why this class can be unit tested
/// without a player, a scene or Unity at all. There is no UnityEngine reference here.
/// </summary>
public class FruitCounterModel : IFruitCounterModel
{
    private readonly int threshold;
    private int current;

    public FruitCounterModel(int threshold)
    {
        // A threshold of zero would lap forever on the first fruit - an endless loop - so
        // it is clamped rather than trusted.
        this.threshold = threshold > 0 ? threshold : 1;
    }

    public int Current { get { return current; } }

    public int Threshold { get { return threshold; } }

    public event Action Changed;

    public event Action ThresholdReached;

    /// <summary>
    /// Eating fruit. A while loop, not an if: a single fruit worth 40 must cost two lives,
    /// not one, and the leftover must carry into the next lap rather than being thrown away.
    /// </summary>
    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        current += amount;

        while (current >= threshold)
        {
            current -= threshold;

            if (ThresholdReached != null)
                ThresholdReached();
        }

        Raise();
    }

    /// <summary>
    /// Back to zero. Always announces, even when it was already zero: the view may be
    /// drawing a stale number from the previous level, and a silent reset would leave it.
    /// </summary>
    public void Reset()
    {
        current = 0;
        Raise();
    }

    private void Raise()
    {
        if (Changed != null)
            Changed();
    }
}
