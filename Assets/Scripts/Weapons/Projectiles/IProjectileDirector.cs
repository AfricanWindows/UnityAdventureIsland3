namespace Game.Projectiles
{
    /// <summary>
    /// ROLE: Director contract: give it a config, get a finished projectile.
    /// PATTERNS: Builder - the Director part.
    /// SOLID: D - the factory never names ProjectileDirector.
    ///
    /// What a factory needs from a director: hand it a config, get a finished projectile.
    ///
    /// It exists so that ConfiguredProjectileFactory names an ABSTRACTION rather than the one
    /// director that happens to exist today (Dependency Inversion). The factory never cared
    /// which recipe ran - only that something runs one - and now the code says so.
    ///
    /// Covariant (out T) for the same reason IFactory&lt;out T&gt; is: TProjectile appears only
    /// as a RESULT, so a director of axes is safely usable wherever a director of
    /// BaseProjectile is expected.
    /// </summary>
    /// <typeparam name="TProjectile">What the recipe produces.</typeparam>
    public interface IProjectileDirector<out TProjectile> where TProjectile : BaseProjectile
    {
        /// <summary>Runs the full recipe and returns the assembled projectile.</summary>
        TProjectile Construct(ProjectileConfigSO config);
    }
}
