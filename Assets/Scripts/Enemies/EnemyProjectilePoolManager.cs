using Game.Weapons;

/// <summary>
/// The pool for enemy fire. Empty for the same engine reason as the player's pools:
/// Unity cannot show a generic MonoBehaviour in the Inspector, so a closed type must
/// exist. All the logic lives once in ProjectilePoolManager&lt;T&gt;.
///
/// Put ONE of these in the scene and every shooting enemy borrows from it - five snakes do
/// not need five pools. It arrives at each of them through GameInstaller, so nothing has to
/// be dragged onto an enemy prefab.
/// </summary>
public class EnemyProjectilePoolManager : ProjectilePoolManager<EnemyProjectile>
{
}
