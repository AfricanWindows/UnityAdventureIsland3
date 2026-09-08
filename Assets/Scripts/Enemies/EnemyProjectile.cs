using Game.Projectiles;
using UnityEngine;

/// <summary>
/// A shot fired BY an enemy - the snake's fireball. It kills the player and ignores other
/// enemies, so enemies can never hurt each other.
///
/// It used to be a whole parallel projectile class: its own Rigidbody handling, its own
/// Destroy(gameObject, lifetime), its own hit test, and no pool at all - the third
/// hierarchy of the same idea after the fireball and the axe. It is now the same
/// DirectionalProjectile everything else uses, and the only thing it still says for itself
/// is WHO it hurts.
///
/// Set Ignore Tag to the enemies' own tag on the prefab so a snake never shoots itself.
/// </summary>
public sealed class EnemyProjectile : DirectionalProjectile
{
    protected override string LogPrefix { get { return "[EnemyShot]"; } }

    /// <summary>
    /// The one inherited step it replaces: enemy fire does not damage IDamageable - that
    /// would let one snake kill another - it kills the player outright, exactly like
    /// touching the enemy itself does.
    /// </summary>
    protected override bool TryHit(Collider2D other)
    {
        IKillable killable;
        if (!other.TryGetComponent(out killable))
            return false;

        killable.Kill();
        return true;
    }
}
