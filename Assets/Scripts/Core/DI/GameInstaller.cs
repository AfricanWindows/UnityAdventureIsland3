using System.Collections.Generic;
using Game.Core.Controls;
using UnityEngine;

namespace Game.Core.DI
{
    /// <summary>
    /// THE COMPOSITION ROOT. The single object in the scene that is allowed to say the
    /// word "new" about a service, and the single place where an abstraction is married to
    /// a concrete implementation.
    ///
    /// Everything else in the game only ever sees interfaces. Swapping the keyboard for a
    /// gamepad, or the real health view for a silent one, is an edit in THIS FILE and
    /// nowhere else (Open/Closed, Dependency Inversion).
    ///
    /// Why DefaultExecutionOrder instead of the Script Execution Order window: the order
    /// requirement is a property of this class, not of the project, so it belongs in the
    /// class. It also survives a fresh clone of the repository, which a project setting a
    /// teammate forgets to commit does not.
    ///
    /// Awake here runs before every other Awake, so an IInjectable is fully injected by the
    /// time its own Awake runs - which is why the components below can cache their services
    /// in Inject() and simply use them afterwards.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public class GameInstaller : MonoBehaviour
    {
        [Header("Scene-bound services")]
        [Tooltip("The health label. Optional - leave empty and the installer looks it up " +
                 "once, here, which is the one place in the project allowed to search the scene.")]
        [SerializeField] private PlayerHealthView healthView;

        [Header("Diagnostics")]
        [Tooltip("Log every registration and every injected component on start-up.")]
        [SerializeField] private bool verbose = true;

        private readonly ServiceContainer _container = new ServiceContainer();

        /// <summary>
        /// Exposed for objects that are created AFTER the scene loaded (a pooled enemy, a
        /// spawned boss). They cannot be injected in Awake because they did not exist yet,
        /// so whoever spawns them calls InjectInto on the new object.
        /// </summary>
        public IServiceContainer Container { get { return _container; } }

        private void Awake()
        {
            RegisterServices();
            InjectSceneObjects();
        }

        /// <summary>
        /// The wiring table. Read top to bottom, this is the complete list of what the game
        /// depends on and what currently satisfies it.
        /// </summary>
        private void RegisterServices()
        {
            // Input: the abstraction is IInputSource, today's implementation is a keyboard.
            _container.Register<IInputSource>(new KeyboardInputSource());

            // The health label lives in the UI canvas, so it is a scene object rather than
            // something we can "new". Resolving it HERE is what lets PlayerHealthController
            // stop calling FindFirstObjectByType - a Service Locator hidden in a controller.
            PlayerHealthView view = healthView;

            if (view == null)
                view = FindAnyObjectByType<PlayerHealthView>(FindObjectsInactive.Include);

            if (view != null)
                _container.Register<IPlayerHealthView>(view);
            else
                Debug.LogWarning("[DI] No PlayerHealthView in the scene - health will not be displayed.", this);

            if (verbose)
                Debug.Log("[DI] Services registered.", this);
        }

        /// <summary>
        /// Hands the container to every component in the scene that asked for one.
        ///
        /// The scan happens ONCE, during Awake, and is the only broad Find in the project.
        /// That is the trade a composition root exists to make: one controlled lookup at
        /// start-up, so that no gameplay class ever has to search for anything again.
        /// </summary>
        private void InjectSceneObjects()
        {
            // Include inactive objects: a weapon that starts switched off still needs its
            // input source before it is ever enabled.
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

            int injected = 0;

            for (int i = 0; i < all.Length; i++)
            {
                IInjectable injectable = all[i] as IInjectable;

                if (injectable == null)
                    continue;

                injectable.Inject(_container);
                injected++;
            }

            if (verbose)
                Debug.Log("[DI] Injected " + injected + " component(s).", this);
        }

        /// <summary>
        /// Injects one freshly created object tree. Call this right after Instantiate, or
        /// from a pool factory, so runtime-spawned objects get the same services the scene
        /// objects were given.
        /// </summary>
        public void InjectInto(GameObject root)
        {
            if (root == null)
                return;

            List<IInjectable> found = new List<IInjectable>();
            root.GetComponentsInChildren(true, found);

            for (int i = 0; i < found.Count; i++)
                found[i].Inject(_container);
        }
    }
}
