using Game.Core;
using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The attack of a shooting animal, sitting on the player and equipped only while he
    /// rides that animal.
    ///
    /// It is a DirectionalWeapon like the axe, so it costs almost nothing: the cooldown, the
    /// facing, the fire point, the pooling and the injected pool are all inherited. The one
    /// difference from the axe is WHO equips it - a mount instead of a weapon slot - and that
    /// difference lives entirely in PlayerMount (Open/Closed).
    ///
    /// ONE class serves both shooting animals, as TWO COMPONENTS on the player with a
    /// different Shot Sprite each. They differ in one image and in nothing else, and a
    /// difference that exists only in the Inspector does not deserve a second class, a second
    /// projectile type and a second pool.
    /// </summary>
    public sealed class AnimalShotWeapon : DirectionalWeapon<AnimalShot>
    {
        [Tooltip("The image this animal's shot flies with. Empty = whatever the projectile " +
                 "prefab already looks like.")]
        [SerializeField] private Sprite shotSprite;

        [Tooltip("Optional override. Normally EMPTY: the pool arrives through injection, " +
                 "so the pool object can live anywhere in the scene.")]
        [SerializeField] private AnimalShotPoolManager shotPool;

        protected override IObjectPool<AnimalShot> ResolveInspectorPool()
        {
            // Compared HERE, where the field still has its concrete Unity type, so Unity's
            // overloaded == applies and a manager deleted from the scene comes back as a
            // real null instead of a corpse.
            return shotPool != null ? shotPool : null;
        }

        /// <summary>
        /// Dress the shot on its way out. This is the whole of "two animals, one pool": the
        /// projectile is told how it looks by whoever fired it, and knows nothing about
        /// animals (Dependency Inversion).
        /// </summary>
        protected override void OnBeforeLaunch(AnimalShot projectile)
        {
            projectile.SetSprite(shotSprite);
        }
    }
}
