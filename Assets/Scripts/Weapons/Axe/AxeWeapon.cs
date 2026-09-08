using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The axe thrower.
    ///
    /// The axe is UNLIMITED once found, exactly as in Adventure Island: there is no ammo
    /// count. What limits a held fire button is the pool - five axes exist, and a sixth
    /// throw waits until one lands. That cap refills by itself, which is why it is a pool
    /// size and not a counter.
    ///
    /// "How you get it" and "how you lose it" are not here either: AxePickable unlocks the
    /// weapon through the shared EquipWeaponPowerUp, and WeaponsLostOnDeath takes it away.
    /// </summary>
    public sealed class AxeWeapon : DirectionalWeapon<ProjectileAxe>
    {
        [Tooltip("The pool that hands out axes. Drag the AxePool object here.")]
        [SerializeField] private AxePoolManager axePool;

        public override WeaponType Type { get { return WeaponType.Axe; } }

        protected override IObjectPool<ProjectileAxe> ResolvePool()
        {
            return axePool;
        }
    }
}
