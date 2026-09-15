using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The axe thrower.
    ///
    /// The axe is UNLIMITED once found, exactly as in Adventure Island: there is no ammo
    /// count. What limits a held fire button is the pool - three axes exist, and a fourth
    /// throw waits until one lands. That cap refills by itself, which is why it is a pool
    /// size and not a counter.
    ///
    /// "How you get it" and "how you lose it" are not here either: AxePickable puts it in
    /// the player's weapon slot through the shared EquipWeaponPowerUp, and the slot takes
    /// it away when he dies.
    ///
    /// Everything about throwing lives in DirectionalWeapon. What is left here is an
    /// optional Inspector pool - normally empty, because the pool arrives from GameInstaller.
    /// </summary>
    public sealed class AxeWeapon : DirectionalWeapon<ProjectileAxe>
    {
        [Tooltip("Optional override. Normally EMPTY: the pool arrives through injection, " +
                 "so the pool object can live anywhere in the scene.")]
        [SerializeField] private AxePoolManager axePool;

        protected override IObjectPool<ProjectileAxe> ResolveInspectorPool()
        {
            // Compared HERE, where the field still has its concrete Unity type, so Unity's
            // overloaded == applies: a manager that was deleted from the scene must come
            // back as a real null. An interface-typed field would not do that, and the
            // weapon would hold a corpse instead of falling back to the injected pool.
            return axePool != null ? axePool : null;
        }
    }
}
