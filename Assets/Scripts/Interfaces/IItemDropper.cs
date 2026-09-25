using UnityEngine;

/// <summary>
/// ROLE: "Put one item into the world" - WHAT drops.
/// PATTERNS: none - a role interface.
/// SOLID: D, O - the triggers (egg, beaten enemy) never name the dropper.
///
/// "Put one item into the world and tell me where it is."
/// The triggers (egg, beaten enemy) depend on this, not on PickableDropper,
/// so WHEN something drops and WHAT drops stay independent (Dependency Inversion, Open/Closed).
/// </summary>
public interface IItemDropper
{
    /// <summary>The dropped item's transform, or null when nothing dropped.</summary>
    Transform DropItem();
}
