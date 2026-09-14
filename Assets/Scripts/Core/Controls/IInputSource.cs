namespace Game.Core.Controls
{
    /// <summary>
    /// What the game needs from the player, expressed as INTENTIONS - "he wants to go
    /// right", "he wants to jump" - never as devices, keys or hardware.
    ///
    /// Before this interface existed, PlayerMovement, PlayerJump and WeaponsHandler each
    /// reached for Keyboard.current themselves. Three classes therefore depended on a
    /// concrete input device, none of them could run in a test, and adding a gamepad meant
    /// editing all of them (Dependency Inversion, Open/Closed).
    ///
    /// Now they depend on this, a gamepad is ONE new implementation registered in
    /// GameInstaller, and a unit test is a fake that returns whatever it likes.
    ///
    /// It once also asked which weapon SLOT was chosen, for the number keys 1-9. The
    /// player carries one weapon and a new one replaces it, so there is nothing to choose
    /// between: those two members were removed rather than left unanswered, and every
    /// input device is two members simpler for it (Interface Segregation).
    /// </summary>
    public interface IInputSource
    {
        /// <summary>-1 left, 0 nothing, +1 right. An axis, so an analogue stick fits later.</summary>
        float Horizontal { get; }

        /// <summary>True only on the frame the jump was requested.</summary>
        bool JumpPressed { get; }

        /// <summary>
        /// True while the player keeps asking to go higher - HELD, not a single frame.
        ///
        /// A STATE and not a "released this frame" EVENT on purpose: an event can fall
        /// between two fixed physics steps and never be seen, and the jump would silently
        /// reach full height. A state read one step late is still the right answer.
        /// </summary>
        bool JumpHeld { get; }

        /// <summary>True only on the frame the trigger was pulled.</summary>
        bool AttackPressed { get; }

        /// <summary>True while the player asks to lie down - HELD, not a single frame.</summary>
        bool CrouchHeld { get; }
    }
}
