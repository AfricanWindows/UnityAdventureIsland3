using UnityEngine;

public class FireballWeapon : MonoBehaviour, IUseableWeapon
{
    [SerializeField] private GameObject projectile;

    private bool _isEquip = false;
    private IFacing facing;

    private void Awake()
    {
        // Asks the owner which way he looks - it never reads his scale itself.
        facing = GetComponentInParent<IFacing>();
    }

    public void Attack()
    {
        if (projectile == null || !_isEquip)
            return;

        GameObject curProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
        ProjectileFireball scProjectile = curProjectile.GetComponent<ProjectileFireball>();
        if (scProjectile != null)
            scProjectile.Attack(facing != null ? facing.FacingDirection : 1f);
    }

    public void Equip()
    {
        _isEquip = true;
    }

    public void UnEquip()
    {
        _isEquip = false;
    }
}
