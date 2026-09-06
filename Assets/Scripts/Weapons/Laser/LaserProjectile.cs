using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE PRODUCT. The laser fills in exactly one of the template's steps - it flies
    /// straight up - and inherits everything else: the shot skeleton, the lifetime timer,
    /// the damage rule, the scenery rule, the pool handshake.
    ///
    /// It used to carry its own copy of "stop at solid geometry". That moved into
    /// BaseProjectile as a tick box, so the laser now says it with data (Stops On Scenery)
    /// instead of code, and the axe reuses the same rule without a line of its own.
    ///
    /// Whether it pierces enemies is NOT decided here either. It comes from the config, so
    /// the same class covers both variants the exercise offers.
    /// </summary>
    public sealed class LaserProjectile : BaseProjectile
    {
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
    }
}
