namespace Game.Core.DI
{
    /// <summary>
    /// The dependency container, described by what a caller is allowed to do with it.
    ///
    /// Two methods: one to fill it, one to read it (Interface Segregation). In practice only
    /// <see cref="GameInstaller"/> ever calls Register - everybody else receives an
    /// already-filled container through <see cref="IInjectable.Inject"/> and only reads.
    ///
    /// The rest of the game depends on THIS interface, never on ServiceContainer, so the
    /// container can be replaced by a test double, or later by a real framework
    /// (VContainer, Zenject), without a single edit anywhere else (Dependency Inversion).
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>Stores one live instance under the type it will be asked for.</summary>
        void Register<T>(T instance) where T : class;

        /// <summary>
        /// The registered instance, or false when nothing is registered. There is no throwing
        /// variant: every object that asks reports a missing service itself, in words that say
        /// what it will not be able to do.
        /// </summary>
        bool TryResolve<T>(out T service) where T : class;
    }
}
