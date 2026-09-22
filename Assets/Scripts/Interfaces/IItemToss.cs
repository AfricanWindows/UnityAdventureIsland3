using UnityEngine;

/// <summary>
/// "Throw this item along a small arc." The egg depends on this, not on ItemToss (Dependency Inversion).
/// </summary>
public interface IItemToss
{
    void Toss(Transform item, float direction);
}
