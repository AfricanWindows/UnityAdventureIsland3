using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// A projectile that is thrown the way its owner is facing.
    ///
    /// It exists so "remember which way I was thrown, turn the sprite to match, fly that
    /// way" is written ONCE, for the axe, the animals' shots and the enemy shot alike (Don't
    /// Repeat Yourself).
    ///
    /// Straight flight lives HERE and not in BaseProjectile: it is what these projectiles have
    /// in common, and the boomerang - which does not fly straight - never inherits it, so it
    /// has nothing to cancel (Liskov Substitution).
    ///
    /// Everything else - the Fire() template, the lifetime timer, the shared IDamageable
    /// hit rule, the pool handshake - is inherited from BaseProjectile and not restated.
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

        /// <summary>
        /// The Fire() step, answered once for every directional shot: move the way it was
        /// thrown. Sealed - a subclass with a different path (the axe's arc) overrides
        /// ApplyMovement, the one part that differs.
        /// </summary>
        protected sealed override void StartMotion()
        {
            ApplyMovement(new Vector2(_facing, 0f));
        }

        /// <summary>
        /// Straight ahead at Stats.Speed. Speed is set ONCE, here, and physics carries the
        /// object from then on. There is deliberately no Update: a per-frame position update
        /// for every projectile is exactly the cost this design is avoiding.
        /// </summary>
        /// <param name="direction">(1, 0) thrown right, (-1, 0) thrown left.</param>
        protected virtual void ApplyMovement(Vector2 direction)
        {
            if (Body != null)
                Body.linearVelocity = direction.normalized * Stats.Speed;
        }
    }
}
