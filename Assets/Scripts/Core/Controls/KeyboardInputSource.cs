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
        // Number keys, in order: weapon 1 answers to Digit1, weapon 2 to Digit2...
        private static readonly Key[] SelectionKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        public int WeaponSlotCount { get { return SelectionKeys.Length; } }

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

        public bool AttackPressed
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.leftCtrlKey.wasPressedThisFrame;
            }
        }

        public bool WeaponSelectPressed(int index)
        {
            if (index < 0 || index >= SelectionKeys.Length)
                return false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return keyboard[SelectionKeys[index]].wasPressedThisFrame;
        }
    }
}
