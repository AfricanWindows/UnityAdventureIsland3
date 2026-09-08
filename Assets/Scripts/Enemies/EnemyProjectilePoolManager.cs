namespace Game.Weapons
{
    /// <summary>
    /// The pool for enemy fire. Empty for the same engine reason as the player's pools:
    /// Unity cannot show a generic MonoBehaviour in the Inspector, so a closed type must
    /// exist. All the logic lives once in ProjectilePoolManager&lt;T&gt;.
    ///
    /// Put ONE of these in each level and point every shooting enemy at it - the shots are
    /// shared, so five snakes do not need five pools.
    /// </summary>
    public class EnemyProjectilePoolManager : ProjectilePoolManager<EnemyProjectile>
    {
    }
}
