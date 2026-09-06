using Game.Core.DI;
using UnityEngine;

namespace Game.Core.Controls
{
    /// <summary>
    /// Base class for every component that reacts to the player: it receives an
    /// <see cref="IInputSource"/> from the composition root and exposes it to the subclass.
    ///
    /// It exists so the injection handshake and the "who forgot the GameInstaller?" guard
    /// are written ONCE instead of in four movement scripts (Don't Repeat Yourself), and so
    /// that no gameplay class ever names a device. A subclass reads intentions - Horizontal,
    /// JumpPressed - and cannot tell a keyboard from a gamepad from a replay file.
    ///
    /// It deliberately declares no Awake/Update of its own: Unity's message methods are
    /// matched by NAME, so a base-class Awake would be silently replaced by a subclass that
    /// declares its own, and the setup would just stop happening. Nothing here needs a
    /// frame, so nothing here claims one.
    /// </summary>
    public abstract class InputDrivenBehaviour : MonoBehaviour, IInjectable
    {
        private IInputSource _inputSource;
        private bool _errorReported;

        /// <summary>True once the composition root has handed over an input source.</summary>
        protected bool HasInput { get { return _inputSource != null; } }

        /// <summary>
        /// The player's intentions. Null only when the scene has no GameInstaller, which is
        /// reported once and then stays quiet - an error repeated every frame hides itself.
        /// </summary>
        protected IInputSource InputSource
        {
            get
            {
                if (_inputSource == null && !_errorReported)
                {
                    _errorReported = true;
                    Debug.LogError("[DI] " + GetType().Name + " was never injected - add a " +
                                   "GameInstaller to the scene.", this);
                }

                return _inputSource;
            }
        }

        /// <summary>
        /// Called by GameInstaller before Awake. Resolved once and cached; the container
        /// itself is NOT kept, so this stays injection and never becomes a Service Locator.
        /// </summary>
        public virtual void Inject(IServiceContainer container)
        {
            if (container != null)
                container.TryResolve(out _inputSource);
        }
    }
}
