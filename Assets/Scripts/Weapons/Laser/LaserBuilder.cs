using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE BUILDER for the laser.
    ///
    /// The prefab and the container arrive through the CONSTRUCTOR - no Resources.Load,
    /// no singleton, no static lookup. That is what makes this class testable and what
    /// keeps the pool manager the single place where the wiring is decided.
    /// </summary>
    public sealed class LaserBuilder : IProjectileBuilder<LaserProjectile>
    {
        private readonly LaserProjectile _prefab;
        private readonly Transform _parent;

        private float _speed;
        private float _lifetime;
        private int _damage;
        private float _scale;
        private bool _piercesEnemies;
        private RuntimeAnimatorController _animatorController;

        public LaserBuilder(LaserProjectile prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            Reset();
        }

        /// <summary>Wipes the accumulated state so one builder can assemble many lasers.</summary>
        public void Reset()
        {
            _speed = 0f;
            _lifetime = 0f;
            _damage = 0;
            _scale = 1f;
            _piercesEnemies = false;
            _animatorController = null;
        }

        public void SetSpeed(float speed) { _speed = speed; }

        public void SetLifetime(float lifetime) { _lifetime = lifetime; }

        public void SetDamage(int damage) { _damage = damage; }

        public void SetSize(float scale) { _scale = scale > 0f ? scale : 1f; }

        public void SetPiercing(bool piercesEnemies) { _piercesEnemies = piercesEnemies; }

        public void SetAnimation(RuntimeAnimatorController controller) { _animatorController = controller; }

        /// <summary>Turns everything collected so far into one finished laser.</summary>
        public LaserProjectile Build()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Laser] LaserBuilder has no prefab - assign the Laser prefab on the LaserPoolManager.");
                return null;
            }

            LaserProjectile laser = Object.Instantiate(_prefab, _parent);

            laser.transform.localScale = Vector3.one * _scale;
            ApplyAnimation(laser);
            laser.Configure(new ProjectileStats(_speed, _lifetime, _damage, _scale, _piercesEnemies));

            return laser;
        }

        private void ApplyAnimation(LaserProjectile laser)
        {
            if (_animatorController == null)
                return;

            Animator animator;
            if (!laser.TryGetComponent(out animator))
                animator = laser.gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = _animatorController;
        }
    }
}
