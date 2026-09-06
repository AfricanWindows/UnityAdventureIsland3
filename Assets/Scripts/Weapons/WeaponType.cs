namespace Game.Weapons
{
    /// <summary>
    /// Names the weapons the new hierarchy knows about. It is an enum rather than a string
    /// so the compiler catches a typo, and so a weapon registry can be a
    /// Dictionary&lt;WeaponType, IWeapon&gt; - an O(1) lookup instead of walking a list.
    ///
    /// Every weapon in the project answers with one of these, including the fireball and
    /// the axe, which were ported onto Game.Weapons.BaseWeapon during the SOLID pass.
    /// </summary>
    public enum WeaponType
    {
        Fireball = 0,
        Axe = 1,
        Laser = 2,
        Boomerang = 3
    }
}
