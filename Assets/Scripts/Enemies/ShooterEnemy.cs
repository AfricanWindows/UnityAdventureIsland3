using UnityEngine;

/// <summary>Exercise item 7: stands still and shoots a fireball every X seconds.</summary>
public class ShooterEnemy : BaseEnemy
{
    [SerializeField] private GameObject projectile;

    [Tooltip("Seconds between shots")]
    [SerializeField] private float shootInterval = 2f;

    [Tooltip("-1 shoots left, 1 shoots right")]
    [SerializeField] private float shootDirection = -1f;

    [Tooltip("Optional: where the shot appears. Empty = the enemy itself.")]
    [SerializeField] private Transform firePoint;

    private float timer = 0f;

    private void Update()
    {
        if (projectile == null || shootInterval <= 0f)
            return;

        timer += Time.deltaTime;

        if (timer < shootInterval)
            return;

        timer = 0f;
        Shoot();
    }

    private void Shoot()
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        GameObject curProjectile = Instantiate(projectile, spawnPosition, Quaternion.identity);

        EnemyProjectile scProjectile = curProjectile.GetComponent<EnemyProjectile>();
        if (scProjectile != null)
            scProjectile.Launch(shootDirection);
    }
}
