using Game.Core.DI;
using UnityEngine;

namespace Game.Core.Controls
{
    /// <summary>
    /// ROLE: Base class of every script that reads the player's input (walk, jump, crouch, attack).
    /// PATTERNS: DI - receives IInputSource in Inject, once for all its subclasses.
    /// SOLID: D - subclasses read intentions, never a device; DRY - the handshake is written once.
    ///
    /// Base class for every component that reacts to the player: it receives an
    /// <see cref="IInputSource"/> from the composition root and exposes it to the subclass.
    ///
    /// It exists so the injection handshake and the "who forgot the GameInstaller?" guard
    /// are written ONCE instead of in each of the four scripts that read the player - walking,
    /// jumping, crouching and the attack button (Don't Repeat Yourself), and so
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

        /// <summary>
        /// True when there is input to act on: the composition root has handed over an input
        /// source, AND the game is running.
        ///
        /// The second half is the pause. Behind the Game Over and Level Complete screens
        /// Time.timeScale is 0, but Update still runs - so a key pressed there used to be
        /// obeyed: an axe left the hand and hung frozen in the air, and a jump pressed there
        /// was remembered and could fire by itself the moment the game restarted. Asked HERE,
        /// once, every component that reads the player stops listening while the game stands
        /// still, and none of them has to know that pausing exists (Don't Repeat Yourself).
        /// The pause itself stays EndScreenController's; this only respects it.
        /// </summary>
        protected bool HasInput { get { return _inputSource != null && Time.timeScale > 0f; } }

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
        public void Inject(IServiceResolver container)
        {
            if (container != null)
                container.TryResolve(out _inputSource);
        }
    }
}
