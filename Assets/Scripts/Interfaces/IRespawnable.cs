using System;

/// <summary>
/// Something that can be defeated and later brought back on the spot where it fell.
///
/// Two members, so a timer that respawns things never learns what an enemy is, what health
/// is, or how one dies (Dependency Inversion, Interface Segregation). Any future object
/// that should come back - a breakable crate, a boss phase - implements these two and gets
/// the existing timer for free.
/// </summary>
public interface IRespawnable
{
    /// <summary>Raised the moment it is defeated.</summary>
    event Action Defeated;

    /// <summary>True while it is out of the game.</summary>
    bool IsDefeated { get; }

    /// <summary>Bring it back, where it currently stands.</summary>
    void Revive();
}
