namespace Game.Core.DI
{
    /// <summary>
    /// The WRITING half of the container, on top of the reading half: fill it, then hand it
    /// out as an <see cref="IServiceResolver"/>.
    ///
    /// Only <see cref="GameInstaller"/> ever holds this type. Everything else is injected
    /// with the resolver, so a gameplay class cannot register or replace a service even by
    /// mistake (Interface Segregation).
    ///
    /// The rest of the game depends on these interfaces, never on ServiceContainer, so the
    /// container can be replaced by a test double, or later by a real framework
    /// (VContainer, Zenject), without a single edit anywhere else (Dependency Inversion).
    /// </summary>
    public interface IServiceContainer : IServiceResolver
    {
        /// <summary>Stores one live instance under the type it will be asked for.</summary>
        void Register<T>(T instance) where T : class;
    }
}
