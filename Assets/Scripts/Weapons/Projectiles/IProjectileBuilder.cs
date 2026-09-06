using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// BUILDER. Knows HOW to put a projectile together, one step at a time - and
    /// deliberately does NOT know which values to use. The numbers arrive from outside.
    ///
    /// That split is the whole point. The version shown in class had a SetSpeed() with no
    /// parameter and a hard-coded 400 inside it, which makes the builder a second place
    /// where balancing lives. Here the builder is reusable for a fast laser, a slow one,
    /// or a boss variant, without editing it.
    /// </summary>
    /// <typeparam name="TProjectile">The concrete product this builder produces.</typeparam>
    public interface IProjectileBuilder<out TProjectile> where TProjectile : BaseProjectile
    {
        /// <summary>Clears everything accumulated so far, so one builder can be reused.</summary>
        void Reset();

        void SetSpeed(float speed);
        void SetLifetime(float lifetime);
        void SetDamage(int damage);
        void SetSize(float scale);
        void SetPiercing(bool piercesEnemies);
        void SetAnimation(RuntimeAnimatorController controller);

        /// <summary>Produces the finished object, or null if it could not be built.</summary>
        TProjectile Build();
    }
}
