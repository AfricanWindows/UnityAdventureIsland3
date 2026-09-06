using System;
using System.Collections.Generic;

namespace Game.Core.DI
{
    /// <summary>
    /// The smallest container that is still a real one: a map from an abstraction to the
    /// single live instance that implements it.
    ///
    /// It is a PLAIN C# class, not a MonoBehaviour and not a singleton. Nothing can reach
    /// it through a static Instance property, which is the whole point - the only way to
    /// get a service out of it is to be HANDED the container, and the only thing that
    /// hands it out is the composition root. That is the difference between dependency
    /// injection and a Service Locator.
    ///
    /// It stores instances rather than factories on purpose: everything this game shares
    /// (input, the game flow, the drain service) is a single long-lived object. Adding
    /// transient registrations later means adding an overload here and changing nothing
    /// else, because callers already talk through IServiceContainer (Open/Closed).
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        // Keyed by the ABSTRACTION, not by the concrete class: Register&lt;IInputSource&gt;(new
        // KeyboardInputSource()) files it under IInputSource, which is the only name the
        // rest of the game knows.
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T instance) where T : class
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "[DI] Cannot register a null " + typeof(T).Name + ".");

            // Loudly, not silently. A second registration of the same type means two
            // installers, or one installer running twice - and whichever object lost the
            // race would keep working while pointing at a service nobody else uses. That
            // bug is invisible at runtime and trivial to catch here.
            if (_services.ContainsKey(typeof(T)))
                throw new InvalidOperationException("[DI] " + typeof(T).Name + " is already registered.");

            _services[typeof(T)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            object service;

            if (!_services.TryGetValue(typeof(T), out service))
                throw new ServiceNotFoundException(typeof(T));

            return (T)service;
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            object found;

            if (_services.TryGetValue(typeof(T), out found))
            {
                service = (T)found;
                return true;
            }

            service = null;
            return false;
        }

        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}
