using System;
using UnityEngine;

public class AxeWeapon : MonoBehaviour, IReloadWeapon, ICounter
{
    [SerializeField] private GameObject projectile;

    [Tooltip("How many axes Mario starts the level with")]
    [SerializeField] private int startAmmo = 0;

    private int ammo;
    private IFacing facing;

    public int Value
    {
        get { return ammo; }
    }

    public event Action<int> OnValueChanged;

    private void Awake()
    {
        ammo = startAmmo;

        // Asks the owner which way he looks - it never reads his scale itself.
        facing = GetComponentInParent<IFacing>();
    }

    private void OnEnable()
    {
        // Tell the UI where to find this counter (see CounterRegistry).
        CounterRegistry.Register(CounterId.Axes, this);
        RaiseValueChanged();
    }

    private void OnDisable()
    {
        CounterRegistry.Unregister(CounterId.Axes, this);
    }

    public void Attack()
    {
        if (projectile == null || ammo <= 0)
            return;

        GameObject curProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
        ProjectileAxe scProjectile = curProjectile.GetComponent<ProjectileAxe>();
        if (scProjectile != null)
            scProjectile.Attack(facing != null ? facing.FacingDirection : 1f);

        ammo--;
        RaiseValueChanged();
    }

    public void AddAmmo(int amount)
    {
        if (amount <= 0)
            return;

        ammo += amount;
        RaiseValueChanged();
    }

    public void Reload()
    {
        AddAmmo(1);
    }

    private void RaiseValueChanged()
    {
        if (OnValueChanged != null)
            OnValueChanged(ammo);
    }
}
