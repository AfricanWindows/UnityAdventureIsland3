using UnityEngine;

/// <summary>
/// The numbers that describe one power bar, and nothing else - no behaviour, no
/// references, no Unity objects.
///
/// Immutable on purpose, exactly like ProjectileStats: a bar that is already draining
/// cannot have its ceiling quietly rewritten by whoever still holds the config asset.
/// The clamping happens once, here, so a designer cannot type a start of 30 into a bar
/// whose maximum is 15 and get a silently broken level.
/// </summary>
[System.Serializable]
public struct PowerStats
{
    [Tooltip("Segments the level starts with. Clamped to Max Power.")]
    [SerializeField] private int startPower;

    [Tooltip("Hard ceiling. Fruit can never push the bar past this.")]
    [SerializeField] private int maxPower;

    [Tooltip("Seconds per lost segment. Level length = Start Power x this.")]
    [SerializeField] private float drainIntervalSeconds;

    public int MaxPower { get { return maxPower < 1 ? 1 : maxPower; } }

    public int StartPower
    {
        get
        {
            if (startPower < 0)
                return 0;

            return startPower > MaxPower ? MaxPower : startPower;
        }
    }

    public float DrainIntervalSeconds
    {
        get { return drainIntervalSeconds > 0.05f ? drainIntervalSeconds : 0.05f; }
    }

    /// <summary>How long the bar lasts untouched, in seconds. Shown in the inspector.</summary>
    public float SecondsOfLife { get { return StartPower * DrainIntervalSeconds; } }

    public PowerStats(int startPower, int maxPower, float drainIntervalSeconds)
    {
        this.startPower = startPower;
        this.maxPower = maxPower;
        this.drainIntervalSeconds = drainIntervalSeconds;
    }
}
