using System;
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
///
/// It is IActivatable, so an ActivateNearPlayer next to it can hold its fire until the player
/// is close. The two classes know nothing about each other - the range calls an interface, and
/// this decides for itself that "asleep" means "stop shooting and start the countdown over".
///
/// It always shoots LEFT, as every enemy in the original game faces left - the way the art is
/// drawn - so there is no direction to set and nothing to flip.
/// </summary>
public class ShooterEnemy : ActivatableEnemy, IInjectable, IAttacker
{
    [Tooltip("Seconds between shots")]
    [SerializeField] private float shootInterval = 2f;

    [Tooltip("Optional: where the shot appears. Empty = the enemy itself.")]
    [SerializeField] private Transform firePoint;

    // Left, the way every enemy faces. A named constant rather than a bare -1 in Shoot().
    private const float ShootDirection = -1f;

    private IObjectPool<EnemyProjectile> pool;
    private Transform muzzle;
    private float timer;

    /// <summary>
    /// Raised the moment a shot leaves. EnemyAnimatorView listens; the snake neither knows it
    /// nor cares, which is why it never grew an Animator field.
    /// </summary>
    public event Action Attacked;

    /// <summary>
    /// Called by GameInstaller before Awake. One pool is shared by every shooting
    /// enemy in the game, so adding a snake costs nothing but the snake - and the snake
    /// knows it only as IObjectPool, never as the manager that holds it (Dependency Inversion).
    /// </summary>
    public void Inject(IServiceResolver container)
    {
        IObjectPool<EnemyProjectile> injected;
        if (container != null && container.TryResolve(out injected))
            pool = injected;
    }

    protected override void OnAwake()
    {
        // Resolved once. Neither of these is looked up while shooting.
        muzzle = firePoint != null ? firePoint : transform;

        if (pool == null)
            Debug.LogError("ShooterEnemy: no enemy shot pool was injected into " + gameObject.name +
                           " - add an EnemyProjectilePoolManager to the scene.", this);
    }

    // A fresh enemy, or one brought back by a restart, waits a full interval before its
    // first shot instead of firing on the frame it appears.
    private void OnEnable()
    {
        timer = 0f;
    }

    /// <summary>
    /// Woken by ActivateNearPlayer: wait a full interval before the first shot, so the player
    /// is never hit by a shot fired the instant he came into range.
    /// </summary>
    protected override void OnActivated()
    {
        timer = 0f;
    }

    private void Update()
    {
        if (!IsActive || pool == null || shootInterval <= 0f)
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

        // Null is the pool's ceiling doing its job - the snake simply skips this shot, and
        // announces nothing, so the mouth does not open on a shot that never happened.
        if (shot == null)
            return;

        shot.Launch(muzzle.position, ShootDirection);

        if (Attacked != null)
            Attacked();
    }
}
