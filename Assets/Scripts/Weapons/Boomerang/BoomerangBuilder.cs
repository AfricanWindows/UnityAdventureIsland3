using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE BUILDER for the boomerang - the exact counterpart of LaserBuilder.
    ///
    /// It implements the SAME IProjectileBuilder&lt;T&gt; the laser uses, so the shared
    /// ProjectileDirector drives it with no changes. The prefab and container arrive
    /// through the CONSTRUCTOR - no Resources.Load, no singleton - which keeps the pool
    /// manager the single place the wiring is decided.
    /// </summary>
    public sealed class BoomerangBuilder : IProjectileBuilder<BoomerangProjectile>
    {
        private readonly BoomerangProjectile _prefab;
        private readonly Transform _parent;

        private float _speed;
        private float _lifetime;
        private int _damage;
        private float _scale;
        private bool _piercesEnemies;
        private RuntimeAnimatorController _animatorController;

        public BoomerangBuilder(BoomerangProjectile prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            Reset();
        }

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

        public BoomerangProjectile Build()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Boomerang] BoomerangBuilder has no prefab - assign the Boomerang prefab on the BoomerangPoolManager.");
                return null;
            }

            BoomerangProjectile boomerang = Object.Instantiate(_prefab, _parent);

            boomerang.transform.localScale = Vector3.one * _scale;
            ApplyAnimation(boomerang);
            boomerang.Configure(new ProjectileStats(_speed, _lifetime, _damage, _scale, _piercesEnemies));

            return boomerang;
        }

        private void ApplyAnimation(BoomerangProjectile boomerang)
        {
            if (_animatorController == null)
                return;

            Animator animator;
            if (!boomerang.TryGetComponent(out animator))
                animator = boomerang.gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = _animatorController;
        }
    }
}
