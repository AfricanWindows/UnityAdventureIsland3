using Game.Core;
using Game.Core.DI;
using Game.Weapons;
using UnityEngine;

/// <summary>
/// An enemy that stands still and shoots at a fixed interval - the fire-breathing snake.
///
/// It borrows its shots from a pool instead of calling Instantiate every couple of
/// seconds, so a level full of snakes allocates nothing while it fires (Pooling System).
/// It does not build, move or destroy the shot; it decides WHEN and WHERE only
/// (Single Responsibility).
/// </summary>
public class ShooterEnemy : BaseEnemy, IInjectable
{
    [Tooltip("Optional override. Normally left empty: the pool arrives through injection, " +
             "so a level full of snakes needs no wiring at all.")]
    [SerializeField] private EnemyProjectilePoolManager shotPool;


    [Tooltip("Seconds between shots")]
    [SerializeField] private float shootInterval = 2f;

    [Tooltip("-1 shoots left, 1 shoots right")]
    [SerializeField] private float shootDirection = -1f;

    [Tooltip("Optional: where the shot appears. Empty = the enemy itself.")]
    [SerializeField] private Transform firePoint;

    private IObjectPool<EnemyProjectile> pool;
    private Transform muzzle;
    private float timer;

    /// <summary>
    /// Called by GameInstaller before Awake. One pool is shared by every shooting
    /// enemy in the game, so adding a snake costs nothing but the snake.
    /// </summary>
    public void Inject(IServiceContainer container)
    {
        if (shotPool != null)
            return;

        IObjectPool<EnemyProjectile> injected;
        if (container != null && container.TryResolve(out injected))
            pool = injected;
    }

    protected override void OnAwake()
    {
        // Resolved once. Neither of these is looked up while shooting.
        if (shotPool != null)
            pool = shotPool;
        muzzle = firePoint != null ? firePoint : transform;

        if (pool == null)
            Debug.LogError("ShooterEnemy: no enemy shot pool - add an " +
                           "EnemyProjectilePoolManager to the scene, or assign one on " +
                           gameObject.name + ".", this);
    }

    // A fresh enemy, or one brought back by a restart, waits a full interval before its
    // first shot instead of firing on the frame it appears.
    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        if (pool == null || shootInterval <= 0f)
            return;

        timer += Time.deltaTime;

        if (timer < shootInterval)
            return;

        timer = 0f;
        Shoot();
    }

    private void Shoot()
    {
        EnemyProjectile shot = pool.Get();

        // Null is the pool's ceiling doing its job - the snake simply skips this shot.
        if (shot == null)
            return;

        shot.Launch(muzzle.position, shootDirection);
    }
}
