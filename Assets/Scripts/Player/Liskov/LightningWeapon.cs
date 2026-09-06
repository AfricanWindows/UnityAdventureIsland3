using UnityEngine;

/// <summary>
/// Liskov demo: still attacks like a BaseWeapon, only louder.
/// It does not change the MEANING of Attack, so it is a valid substitute.
/// </summary>
public class LightningWeapon : BaseWeapon
{
    private bool isLightOn = false;

    public override void Attack()
    {
        Debug.Log("LongRangeWeapon " + isLightOn);
        base.Attack();
    }
}
