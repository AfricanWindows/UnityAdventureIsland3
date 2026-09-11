namespace Game.Weapons
{
    /// <summary>
    /// Names the weapons the game knows about. An enum rather than a string so the compiler
    /// catches a typo, and so a weapon registry can be a Dictionary&lt;WeaponType, IWeapon&gt;
    /// - an O(1) lookup instead of walking a list.
    ///
    /// The assignment asks for two throwables, and these are they. Fireball and Laser were
    /// removed along with the weapons themselves: both were unreachable in the levels.
    /// </summary>
    public enum WeaponType
    {
        Axe = 1,
        Boomerang = 3
    }
}
