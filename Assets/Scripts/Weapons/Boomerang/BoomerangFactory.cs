using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE CREATOR - the exact counterpart of LaserFactory. Create() is the factory
    /// method: it decides the product is a BoomerangProjectile and hides the whole
    /// director/builder/Instantiate chain behind that one call.
    ///
    /// It reuses the shared ProjectileConfigSO - no boomerang-specific config type. The
    /// pool only ever sees IFactory&lt;BoomerangProjectile&gt;.
    /// </summary>
    public sealed class BoomerangFactory : ProjectileFactory<BoomerangProjectile>
    {
        private readonly BoomerangDirector _director;
        private readonly ProjectileConfigSO _config;

        public BoomerangFactory(BoomerangDirector director, ProjectileConfigSO config)
        {
            _director = director;
            _config = config;
        }

        public override BoomerangProjectile Create()
        {
            if (_director == null || _config == null)
            {
                Debug.LogError("[Boomerang] BoomerangFactory is missing its director or its config asset.");
                return null;
            }

            return _director.Construct(_config);
        }
    }
}
