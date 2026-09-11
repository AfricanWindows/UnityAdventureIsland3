using Game.Core;

namespace Game.Projectiles
{
    /// <summary>
    /// FACTORY METHOD - the Creator half.
    ///
    /// Mapping onto the pattern as taught:
    ///   Product          = BaseProjectile
    ///   Concrete Product = ProjectileAxe
    ///   Creator          = ProjectileFactory&lt;T&gt;   (this class)
    ///   Concrete Creator = ConfiguredProjectileFactory&lt;T&gt;
    ///   Factory Method   = Create()
    ///
    /// The rest of the game asks "give me a laser" and learns nothing about builders,
    /// step order, or Instantiate.
    /// </summary>
    public abstract class ProjectileFactory<TProjectile> : IFactory<TProjectile>
        where TProjectile : BaseProjectile
    {
        /// <summary>The factory method. A concrete creator decides what comes out.</summary>
        public abstract TProjectile Create();
    }
}
