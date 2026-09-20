namespace Game.Core.DI
{
    /// <summary>
    /// The READING half of the container: "do you have one of these?".
    ///
    /// This is what an injected class is handed. It can ask for what it needs and nothing
    /// else - it cannot register a service, replace one, or even see that registering is
    /// possible. Filling the container is the composition root's job alone, and the type
    /// system now says so (Interface Segregation).
    ///
    /// TryResolve and not Resolve: a missing service is a normal answer here. A camera with
    /// no player still has to run, and each class decides for itself whether to warn, to
    /// fall back on an Inspector reference, or to go quiet.
    /// </summary>
    public interface IServiceResolver
    {
        /// <summary>
        /// Hands over the registered T, or false when nobody registered one.
        /// </summary>
        bool TryResolve<T>(out T service) where T : class;
    }
}
