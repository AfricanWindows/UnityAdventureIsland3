using UnityEngine;

/// <summary>
/// ROLE: "Throw this item along a small arc."
/// PATTERNS: none - a role interface.
/// SOLID: D - the egg never names ItemToss.
///
/// "Throw this item along a small arc." The egg depends on this, not on ItemToss (Dependency Inversion).
/// </summary>
public interface IItemToss
{
    void Toss(Transform item, float direction);
}
