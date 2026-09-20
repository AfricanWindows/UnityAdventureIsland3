using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// A projectile that is thrown the way its owner is facing.
    ///
    /// It exists so "remember which way I was thrown, turn the sprite to match, fly that
    /// way" is written ONCE, for the axe and the enemy shot alike (Don't Repeat Yourself).
    ///
    /// Everything else - the Fire() template, the lifetime timer, the shared IDamageable
    /// hit rule, the pool handshake - is inherited from BaseProjectile and not restated.
    ///
    /// It is still abstract: "which way" is answered here, "how fast and along what path"
    /// is not, and a class that answers only half a question should not be instantiable.
    /// </summary>
    public abstract class DirectionalProjectile : BaseProjectile
    {
        private float _facing = 1f;

        /// <summary>
        /// The weapon's single call: record the direction, then hand over to the inherited
        /// Fire() template, which keeps the step order for every projectile in the game.
        /// </summary>
        public void Launch(Vector3 origin, float facing)
        {
            _facing = facing >= 0f ? 1f : -1f;
            Fire(origin);
        }

        /// <summary>Turn the sprite to face its flight. Runs inside the Fire() template.</summary>
        protected override void OnBeforeFire()
        {
            // Multiplies the ABSOLUTE scale, so the size the builder applied survives a
            // left-hand throw instead of being flipped away.
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * _facing;
            transform.localScale = scale;
        }

        /// <summary>Straight ahead. A subclass with an arc overrides ApplyMovement instead.</summary>
        protected override Vector2 GetDirection()
        {
            return new Vector2(_facing, 0f);
        }
    }
}
