namespace Game.Core.DI
{
    /// <summary>
    /// ROLE: "Give me my dependencies" - the constructor stand-in for a MonoBehaviour.
    /// PATTERNS: DI - method injection, called by GameInstaller before Awake.
    /// SOLID: I - one method.
    ///
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
        /// Called once by <see cref="GameInstaller"/>, before any Awake. Resolve here, cache
        /// here, and never keep the resolver itself - holding on to it would turn injection
        /// back into a Service Locator.
        ///
        /// It receives the READING half of the container: a component asks for what it needs
        /// and cannot register anything.
        ///
        /// SUBSCRIBING HERE IS ALLOWED, and a few components do it. The reason is the timing:
        /// this runs before every Awake, so a listener that subscribes here cannot miss an
        /// event, while one that waits for OnEnable can. The price is that it must let go in
        /// OnDestroy rather than in OnDisable - OnEnable/OnDisable is the symmetric pair, and
        /// a subscription made here has no OnDisable to match it. What must NOT happen here is
        /// anything that CHANGES the world - moving objects, starting timers, showing panels.
        /// Injection is wiring, and the game has not started yet.
        /// </summary>
        void Inject(IServiceResolver services);
    }
}
