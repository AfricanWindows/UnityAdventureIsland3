using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE CREATOR - one class for every projectile, replacing the hand-written
    /// factory each weapon used to have, which differed only in a type name.
    ///
    /// Mapping onto the pattern as taught:
    ///   Product          = BaseProjectile
    ///   Concrete Product = ProjectileAxe, BoomerangProjectile, ...
    ///   Creator          = ProjectileFactory&lt;T&gt;            (abstract)
    ///   Concrete Creator = ConfiguredProjectileFactory&lt;T&gt;   (this class)
    ///   Factory Method   = Create()
    ///
    /// Create() hides the entire director/builder/Instantiate chain behind one call. The
    /// pool only ever sees IFactory&lt;T&gt;, so it can be handed a test double or an
    /// addressables loader and never notice (Dependency Inversion).
    ///
    /// The director and the config arrive through the constructor: this class chooses
    /// nothing and looks nothing up.
    /// </summary>
    public class ConfiguredProjectileFactory<TProjectile> : ProjectileFactory<TProjectile>
        where TProjectile : BaseProjectile
    {
        private readonly ProjectileDirector<TProjectile> _director;
        private readonly ProjectileConfigSO _config;
        private readonly string _logPrefix;

        public ConfiguredProjectileFactory(ProjectileDirector<TProjectile> director, ProjectileConfigSO config)
        {
            _director = director;
            _config = config;
            _logPrefix = "[" + typeof(TProjectile).Name + "]";
        }

        /// <summary>The factory method: run the recipe, hand back the product.</summary>
        public override TProjectile Create()
        {
            if (_director == null || _config == null)
            {
                Debug.LogError(_logPrefix + " factory is missing its director or its config asset.");
                return null;
            }

            return _director.Construct(_config);
        }
    }
}
