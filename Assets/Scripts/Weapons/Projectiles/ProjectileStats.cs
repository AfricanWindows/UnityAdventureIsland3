using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// The numbers that describe one projectile, and nothing else - no behaviour, no
    /// references, no Unity objects. That is what makes it safe to copy into a struct
    /// and hand around without allocating.
    ///
    /// It is immutable on purpose: a projectile that is already in flight cannot have its
    /// damage quietly rewritten by whoever still holds the config.
    /// </summary>
    [System.Serializable]
    public struct ProjectileStats
    {
        [Tooltip("Units per second")]
        [SerializeField] private float speed;

        [Tooltip("Seconds before the projectile returns itself to the pool")]
        [SerializeField] private float lifetime;

        [SerializeField] private int damage;

        [Tooltip("Uniform scale applied to the projectile object")]
        [SerializeField] private float scale;

        [Tooltip("True = flies through enemies, false = disappears on the first hit")]
        [SerializeField] private bool piercesEnemies;

        public float Speed { get { return speed; } }
        public float Lifetime { get { return lifetime; } }
        public int Damage { get { return damage; } }
        public float Scale { get { return scale; } }
        public bool PiercesEnemies { get { return piercesEnemies; } }

        public ProjectileStats(float speed, float lifetime, int damage, float scale, bool piercesEnemies)
        {
            this.speed = speed;
            this.lifetime = lifetime;
            this.damage = damage;
            this.scale = scale;
            this.piercesEnemies = piercesEnemies;
        }
    }
}
