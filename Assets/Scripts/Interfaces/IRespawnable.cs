using System;

/// <summary>
/// Something that can be taken out of the game - an enemy beaten, a fruit eaten, an egg
/// opened - and later brought back.
///
/// Three members, so a timer that respawns things never learns what an enemy, a fruit or an
/// egg is, what health is, or how one dies (Dependency Inversion, Interface Segregation).
/// That is why the one RespawnTimer serves all of them.
/// </summary>
public interface IRespawnable
{
    /// <summary>Raised the moment it is taken out of the game.</summary>
    event Action Defeated;

    /// <summary>True while it is out of the game.</summary>
    bool IsDefeated { get; }

    /// <summary>Bring it back.</summary>
    void Revive();
}
