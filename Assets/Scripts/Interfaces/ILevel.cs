using UnityEngine;

/// <summary>
/// ROLE: One level: its name, its spawn point, switch it on or off.
/// PATTERNS: none - a role interface.
/// SOLID: D - the level flow never names Level.
///
/// One playable level: switch it on or off and tell where the player starts.
/// The level flow and the pools depend on this, not on the concrete Level (Dependency Inversion).
/// </summary>
public interface ILevel
{
    /// <summary>Name for the console.</summary>
    string DisplayName { get; }

    /// <summary>Where the player belongs in this level.</summary>
    Vector3 SpawnPosition { get; }

    void Activate();

    void Deactivate();
}
