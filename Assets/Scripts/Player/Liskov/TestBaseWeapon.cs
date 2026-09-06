using UnityEngine;

public class TestBaseWeapon : MonoBehaviour
{
    void Start()
    {
        BaseWeapon bs = new BaseWeapon();
        AttackEnemy(bs);
        LightningWeapon bslw = new LightningWeapon();
        AttackEnemy(bslw);
    }
    public void AttackEnemy(BaseWeapon attackingWeapon)
    {
        attackingWeapon.Attack();
    }
}
