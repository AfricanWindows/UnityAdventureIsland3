using UnityEngine;

/// <summary>
/// Anything a projectile can hurt. Projectiles talk to this interface,
/// so they never need to know what kind of enemy they hit.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
