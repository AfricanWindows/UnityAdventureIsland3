using System;

/// <summary>
/// MODEL contract of the health feature (exercise item 2).
/// The controller talks to this interface, never to the concrete class.
/// </summary>
public interface IPlayerHealthModel
{
    int Current { get; }
    int Max { get; }
    bool IsFull { get; }

    /// <summary>Raised every time the health value changes.</summary>
    event Action Changed;

    /// <summary>Raised once when health reaches zero.</summary>
    event Action Empty;

    /// <summary>Collecting a heart. False when already at the maximum.</summary>
    bool Add(int amount);

    /// <summary>Landing on spikes.</summary>
    void Remove(int amount);
}
