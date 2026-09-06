using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE PRODUCT. The laser fills in exactly two of the template's steps - it flies
    /// straight up, and it is stopped by solid geometry - and inherits everything else:
    /// the shot skeleton, the lifetime timer, the damage rule, the pool handshake.
    ///
    /// Whether it pierces enemies is NOT decided here. It comes from the config, so the
    /// same class covers both variants the exercise offers.
    /// </summary>
    public sealed class LaserProjectile : BaseProjectile
    {
        [Tooltip("Layers that stop the laser (ground, ceiling). Leave empty and ANY solid " +
                 "non-trigger collider stops it, which is what this project needs today.")]
        [SerializeField] private LayerMask blockingLayers;

        [Tooltip("Z rotation applied when fired. Use 90 if the sprite is drawn horizontally.")]
        [SerializeField] private float spriteRotationZ;

        protected override string LogPrefix { get { return "[Laser]"; } }

        /// <summary>Straight up, always - that is the whole brief for this weapon.</summary>
        protected override Vector2 GetDirection()
        {
            return Vector2.up;
        }

        protected override Quaternion GetRotation()
        {
            return Quaternion.Euler(0f, 0f, spriteRotationZ);
        }

        protected override bool IsBlockedBy(Collider2D other)
        {
            // Triggers are coins, power-ups and checkpoints - the laser flies through those.
            if (other.isTrigger)
                return false;

            // No mask configured yet (this project has no Ground layer): treat every solid
            // collider as a wall. Once a Ground layer exists, set the mask and the laser
            // stops only there - fewer collision checks, same behaviour.
            if (blockingLayers.value == 0)
                return true;

            return (blockingLayers.value & (1 << other.gameObject.layer)) != 0;
        }
    }
}
