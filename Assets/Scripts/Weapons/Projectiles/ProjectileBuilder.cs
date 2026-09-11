using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE BUILDER - one class for every projectile in the game.
    ///
    /// It replaced a hand-written builder per weapon, which were byte-for-byte identical
    /// apart from a type name and a log prefix: 85 lines each, differing in eleven. With
    /// six weapons planned that scheme was heading for roughly five hundred lines of
    /// copy-paste, and a bug fixed in one copy silently surviving in the other five.
    ///
    /// Nothing was lost by merging them, because a builder assembles a projectile - it
    /// never decides how one FLIES. That difference lives in the concrete product
    /// (ProjectileAxe, BoomerangProjectile), which is where behaviour belongs.
    ///
    /// Adding a weapon now adds no builder at all (Open/Closed): the generic argument
    /// changes and the code does not.
    ///
    /// The prefab and the container arrive through the CONSTRUCTOR - no Resources.Load,
    /// no singleton, no static lookup. That is what makes this class testable, and what
    /// keeps the pool manager the single place where the wiring is decided.
    /// </summary>
    /// <typeparam name="TProjectile">The concrete product being assembled.</typeparam>
    public class ProjectileBuilder<TProjectile> : IProjectileBuilder<TProjectile>
        where TProjectile : BaseProjectile
    {
        private readonly TProjectile _prefab;
        private readonly Transform _parent;

        // Named after the product, so the console still says [ProjectileAxe] rather than
        // a generic "[Projectile]" - the merge cost nothing in diagnosability.
        private readonly string _logPrefix;

        private float _speed;
        private float _lifetime;
        private int _damage;
        private float _scale;
        private bool _piercesEnemies;
        private RuntimeAnimatorController _animatorController;

        public ProjectileBuilder(TProjectile prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
            _logPrefix = "[" + typeof(TProjectile).Name + "]";

            Reset();
        }

        /// <summary>Wipes the accumulated state so one builder can assemble many objects.</summary>
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

        /// <summary>A scale of zero would make the projectile invisible, so 0 means "normal".</summary>
        public void SetSize(float scale) { _scale = scale > 0f ? scale : 1f; }

        public void SetPiercing(bool piercesEnemies) { _piercesEnemies = piercesEnemies; }

        public void SetAnimation(RuntimeAnimatorController controller) { _animatorController = controller; }

        /// <summary>Turns everything collected so far into one finished projectile.</summary>
        public TProjectile Build()
        {
            if (_prefab == null)
            {
                Debug.LogError(_logPrefix + " ProjectileBuilder has no prefab - assign it on the pool manager.");
                return null;
            }

            TProjectile item = Object.Instantiate(_prefab, _parent);

            item.transform.localScale = Vector3.one * _scale;
            ApplyAnimation(item);
            item.Configure(new ProjectileStats(_speed, _lifetime, _damage, _scale, _piercesEnemies));

            return item;
        }

        /// <summary>
        /// Optional step: a projectile with no animator controller stays a plain sprite,
        /// and one that already has an Animator keeps it instead of gaining a second.
        /// </summary>
        private void ApplyAnimation(TProjectile item)
        {
            if (_animatorController == null)
                return;

            Animator animator;
            if (!item.TryGetComponent(out animator))
                animator = item.gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = _animatorController;
        }
    }
}
