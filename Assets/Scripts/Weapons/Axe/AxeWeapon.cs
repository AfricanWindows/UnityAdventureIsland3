namespace Game.Weapons
{
    /// <summary>
    /// ROLE: The axe thrower.
    /// PATTERNS: Template Method - every step is inherited from DirectionalWeapon; Pooling.
    ///
    /// The axe thrower.
    ///
    /// The axe is UNLIMITED once found, exactly as in Adventure Island: there is no ammo
    /// count. What limits a held fire button is the pool - its Max Size is how many axes
    /// exist, and one more throw waits until one of them lands. That cap refills by itself,
    /// which is why it is a pool size and not a counter.
    ///
    /// "How you get it" and "how you lose it" are not here either: AxePickable puts it in
    /// the player's weapon slot through the shared EquipWeaponPowerUp, and the slot takes
    /// it away when he dies.
    ///
    /// Everything about throwing lives in DirectionalWeapon and ProjectileWeapon, and the pool
    /// arrives from GameInstaller. It is empty on purpose: Unity cannot put an open generic
    /// MonoBehaviour on a GameObject, so the closed type needs a class of its own - the same
    /// reason PickableDropper is empty.
    /// </summary>
    public sealed class AxeWeapon : DirectionalWeapon<ProjectileAxe>
    {
    }
}
