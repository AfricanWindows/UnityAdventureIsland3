using System;

/// <summary>
/// ROLE: Announces "I was taken out of the game" (the Defeated event).
/// PATTERNS: Observer - the event.
/// SOLID: I - split from IRespawnable, because DropOnDefeat needs only this.
///
/// Something that can be taken out of the game - an enemy beaten, a fruit eaten, an egg
/// opened - and says so the moment it happens.
///
/// Only the NEWS, nothing about coming back. DropOnDefeat needs to hear that an enemy fell
/// and nothing more, so it depends on this and not on IRespawnable - otherwise it would be
/// tied to Revive(), which it never calls, and something that is beaten for good could not
/// drop loot without writing an empty Revive() to satisfy it (Interface Segregation).
/// </summary>
public interface IDefeatable
{
    /// <summary>Raised the moment it is taken out of the game.</summary>
    event Action Defeated;
}
