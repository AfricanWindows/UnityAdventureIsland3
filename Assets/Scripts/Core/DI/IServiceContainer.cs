using System;

namespace Game.Core.DI
{
    /// <summary>
    /// The dependency container, described by what a caller is allowed to do with it.
    ///
    /// Deliberately three methods (Interface Segregation): a class that only needs to READ
    /// a service should never be handed something that can also re-register it. In practice
    /// only <see cref="GameInstaller"/> ever calls Register - everybody else receives an
    /// already-filled container through <see cref="IInjectable.Inject"/>.
    ///
    /// The rest of the game depends on THIS interface, never on ServiceContainer, so the
    /// container can be replaced by a test double, or later by a real framework
    /// (VContainer, Zenject), without a single edit anywhere else (Dependency Inversion).
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>Stores one live instance under the type it will be asked for.</summary>
        void Register<T>(T instance) where T : class;

        /// <summary>The registered instance. Throws when nothing is registered - a missing
        /// service is a wiring bug, and a bug that shouts is cheaper than a silent null.</summary>
        T Resolve<T>() where T : class;

        /// <summary>Non-throwing variant, for genuinely optional services.</summary>
        bool TryResolve<T>(out T service) where T : class;

        /// <summary>True when something is registered for T.</summary>
        bool IsRegistered<T>() where T : class;
    }

    /// <summary>Thrown when a required service was never registered.</summary>
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(Type type)
            : base("[DI] No service registered for " + type.Name +
                   ". Register it in GameInstaller before anything asks for it.")
        {
        }
    }
}
