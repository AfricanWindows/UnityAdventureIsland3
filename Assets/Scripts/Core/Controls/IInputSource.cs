namespace Game.Core.Controls
{
    /// <summary>
    /// What the game needs from the player, expressed as INTENTIONS - "he wants to go
    /// right", "he wants to jump" - never as devices, keys or hardware.
    ///
    /// Before this interface existed, PlayerMovement, PlayerJump, PlayerDoubleJump and
    /// WeaponsHandler each reached for Keyboard.current themselves. Four classes therefore
    /// depended on a concrete input device, none of them could run in a test, and adding a
    /// gamepad meant editing all four (Dependency Inversion, Open/Closed).
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

        /// <summary>True only on the frame the trigger was pulled.</summary>
        bool AttackPressed { get; }

        /// <summary>True while the player asks to lie down - HELD, not a single frame.</summary>
        bool CrouchHeld { get; }
    }
}
