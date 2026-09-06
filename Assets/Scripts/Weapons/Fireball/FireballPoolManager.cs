namespace Game.Weapons
{
    /// <summary>
    /// The fireball's pool. Empty for the same reason LaserPoolManager is: Unity does not
    /// serialise the fields of a generic MonoBehaviour, so the Inspector needs a closed
    /// type. All the logic lives once in ProjectilePoolManager&lt;T&gt;.
    ///
    /// Its existence is the point of the whole refactor: adding the fireball to the pooled
    /// architecture cost this one line, where before it would have cost a builder, a
    /// director, a factory and a pool manager - four near-identical files.
    /// </summary>
    public class FireballPoolManager : ProjectilePoolManager<ProjectileFireball>
    {
    }
}
