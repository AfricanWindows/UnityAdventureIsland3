namespace Game.Weapons
{
    /// <summary>
    /// Names the weapons the new hierarchy knows about. It is an enum rather than a string
    /// so the compiler catches a typo, and so a weapon registry can be a
    /// Dictionary&lt;WeaponType, IWeapon&gt; - an O(1) lookup instead of walking a list.
    ///
    /// The older FireballWeapon and AxeWeapon do not use it: they predate this hierarchy
    /// and are listed here only so the names line up when they are eventually ported.
    /// </summary>
    public enum WeaponType
    {
        Fireball = 0,
        Axe = 1,
        Laser = 2,
        Boomerang = 3
    }
}
