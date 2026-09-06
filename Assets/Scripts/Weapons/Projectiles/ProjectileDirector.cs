namespace Game.Projectiles
{
    /// <summary>
    /// DIRECTOR. Owns the recipe: which steps run, and in which order. It never touches a
    /// prefab and never decides a number - it reads them off the config and dictates the
    /// sequence to whatever builder it was handed.
    ///
    /// Swap the builder and the very same recipe produces a different representation - a
    /// laser instead of a fireball, or a debug projectile with gizmos - with no change here.
    /// </summary>
    public class ProjectileDirector<TProjectile> where TProjectile : BaseProjectile
    {
        private readonly IProjectileBuilder<TProjectile> _builder;

        public ProjectileDirector(IProjectileBuilder<TProjectile> builder)
        {
            _builder = builder;
        }

        /// <summary>Runs the full recipe and returns the assembled projectile.</summary>
        public virtual TProjectile Construct(ProjectileConfigSO config)
        {
            if (_builder == null || config == null)
                return null;

            ProjectileStats stats = config.Stats;

            _builder.Reset();
            _builder.SetSpeed(stats.Speed);
            _builder.SetLifetime(stats.Lifetime);
            _builder.SetDamage(stats.Damage);
            _builder.SetSize(stats.Scale);
            _builder.SetPiercing(stats.PiercesEnemies);
            _builder.SetAnimation(config.AnimatorController);

            return _builder.Build();
        }
    }
}
