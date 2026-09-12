using UnityEngine.InputSystem;

namespace Game.Core.Controls
{
    /// <summary>
    /// The ONE place in the project that knows a keyboard exists.
    ///
    /// A plain C# class, not a MonoBehaviour: reading a device is not something that needs
    /// a transform or an Update - the caller already has one. GameInstaller creates it once
    /// and registers it as IInputSource; nothing else ever names this type.
    ///
    /// Replacing it with a GamepadInputSource, a replay file or a recorded test script is a
    /// one-line change in GameInstaller (Open/Closed).
    /// </summary>
    public class KeyboardInputSource : IInputSource
    {
        /// <summary>Both WASD and the arrow keys, resolved to a single axis.</summary>
        public float Horizontal
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                    return 0f;

                float axis = 0f;

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    axis = -1f;

                // Checked second so holding both keys resolves to "right", exactly the
                // behaviour the movement code had before it was extracted.
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    axis = 1f;

                return axis;
            }
        }

        public bool JumpPressed
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            }
        }

        /// <summary>Down arrow or S, held. Both, because both are muscle memory.</summary>
        public bool CrouchHeld
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                return keyboard != null &&
                       (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
            }
        }

        /// <summary>
        /// Left Ctrl throws whatever the player is carrying. One button, because he has
        /// one weapon - the number keys that used to pick a slot are gone with the slots.
        /// </summary>
        public bool AttackPressed
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.leftCtrlKey.wasPressedThisFrame;
            }
        }
    }
}
