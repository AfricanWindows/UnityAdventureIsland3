using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE CREATOR. Create() is the factory method: it decides that the product is a
    /// LaserProjectile and hides the entire director/builder/Instantiate chain behind it.
    ///
    /// The pool only ever sees IFactory&lt;LaserProjectile&gt;, so it can be handed a
    /// completely different implementation - a test double, an addressables loader - and
    /// never notice (Dependency Inversion).
    /// </summary>
    public sealed class LaserFactory : ProjectileFactory<LaserProjectile>
    {
        private readonly LaserDirector _director;
        private readonly ProjectileConfigSO _config;

        public LaserFactory(LaserDirector director, ProjectileConfigSO config)
        {
            _director = director;
            _config = config;
        }

        public override LaserProjectile Create()
        {
            if (_director == null || _config == null)
            {
                Debug.LogError("[Laser] LaserFactory is missing its director or its config asset.");
                return null;
            }

            return _director.Construct(_config);
        }
    }
}
