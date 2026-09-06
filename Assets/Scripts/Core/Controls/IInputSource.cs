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
    /// </summary>
    public interface IInputSource
    {
        /// <summary>-1 left, 0 nothing, +1 right. An axis, so an analogue stick fits later.</summary>
        float Horizontal { get; }

        /// <summary>True only on the frame the jump was requested.</summary>
        bool JumpPressed { get; }

        /// <summary>True only on the frame the trigger was pulled.</summary>
        bool AttackPressed { get; }

        /// <summary>True on the frame weapon number <paramref name="index"/> was picked.</summary>
        bool WeaponSelectPressed(int index);

        /// <summary>How many weapon slots this device can address at all.</summary>
        int WeaponSlotCount { get; }
    }
}
