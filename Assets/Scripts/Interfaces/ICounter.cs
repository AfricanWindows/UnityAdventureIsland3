using System;

/// <summary>
/// Any game system that holds a number the UI can display (coins, lives, axes...).
/// The UI depends on this abstraction, not on a concrete manager class.
/// </summary>
public interface ICounter
{
    int Value { get; }

    event Action<int> OnValueChanged;
}
