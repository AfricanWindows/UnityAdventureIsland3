using Game.Projectiles;
using UnityEngine;

/// <summary>
/// A shot fired BY an enemy - the snake's shot. It kills the player and ignores other
/// enemies, so enemies can never hurt each other.
///
/// It is the same DirectionalProjectile the axe uses - pooled, with the same lifetime and
/// the same flight - and the only thing it says for itself is WHO it hurts.
///
/// Set Ignore Tag to the enemies' own tag on the prefab so a snake never shoots itself.
/// </summary>
public sealed class EnemyProjectile : DirectionalProjectile
{
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

        // The animal he is riding can eat the shot. True either way: the shot hit something
        // and is spent, it simply cost an animal instead of a life.
        if (other.gameObject.TryAbsorbHit(gameObject))
            return true;

        killable.Kill();
        return true;
    }
}
