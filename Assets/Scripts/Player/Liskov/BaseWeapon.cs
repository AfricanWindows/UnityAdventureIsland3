using UnityEngine;

/// <summary>
/// Liskov demo: the base weapon every other weapon must be able to replace.
/// </summary>
public class BaseWeapon
{
    private int range = 5;
    private int damage = 10;

    public virtual void Attack()
    {
        Debug.Log("BaseWeapon Attack, " + range + "," + damage);
    }
}
