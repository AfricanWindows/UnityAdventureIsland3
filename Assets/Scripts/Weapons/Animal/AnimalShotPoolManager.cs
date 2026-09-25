using Game.Projectiles;

namespace Game.Weapons
{
    /// <summary>
    /// ROLE: The pool both shooting animals borrow from.
    /// PATTERNS: Pooling; Generics - an empty closed type of ProjectilePoolManager.
    ///
    /// The one pool both shooting animals borrow from.
    ///
    /// Empty, like AxePoolManager: Unity cannot put an open generic MonoBehaviour on a
    /// GameObject, so a closed subclass is what the scene object needs. The builder, the
    /// director, the factory and the pool itself all live in ProjectilePoolManager.
    ///
    /// One pool for both animals also means one ammo limit between them - which costs
    /// nothing, since the player can only ever ride one of them at a time.
    /// </summary>
    public class AnimalShotPoolManager : ProjectilePoolManager<AnimalShot>
    {
    }
}
