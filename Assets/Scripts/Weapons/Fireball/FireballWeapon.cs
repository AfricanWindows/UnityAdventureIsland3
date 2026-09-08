using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The fire flower's gun. Everything about throwing - the cooldown, the unlock gate,
    /// borrowing from the pool, aiming the way Mario faces - is inherited. All this class
    /// says is WHICH pool and WHICH weapon it is.
    /// </summary>
    public sealed class FireballWeapon : DirectionalWeapon<ProjectileFireball>
    {
        [Tooltip("The pool that hands out fireballs. Drag the FireballPool object here.")]
        [SerializeField] private FireballPoolManager fireballPool;

        public override WeaponType Type { get { return WeaponType.Fireball; } }

        protected override IObjectPool<ProjectileFireball> ResolvePool()
        {
            return fireballPool;
        }
    }
}
