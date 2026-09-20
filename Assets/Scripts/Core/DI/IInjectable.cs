namespace Game.Core.DI
{
    /// <summary>
    /// "I have dependencies that arrive from outside."
    ///
    /// A MonoBehaviour cannot have a constructor - Unity builds it - so this method is the
    /// constructor's stand-in. The composition root calls it once, before Awake, and the
    /// component pulls out exactly what it needs and keeps it in a readonly-in-spirit field.
    ///
    /// The interface is one method (Interface Segregation) and says nothing about WHICH
    /// services exist, so adding a new service never touches this file (Open/Closed).
    /// </summary>
    public interface IInjectable
    {
        /// <summary>
        /// Called once by <see cref="GameInstaller"/>. Resolve here, cache here, and never
        /// keep the resolver itself - holding on to it would turn injection back into a
        /// Service Locator.
        ///
        /// It receives the READING half of the container: a component asks for what it needs
        /// and cannot register anything.
        /// </summary>
        void Inject(IServiceResolver services);
    }
}
