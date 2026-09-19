using System;
using Game.Core;
using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// TEMPLATE METHOD. Every projectile is fired the same way - place it, push it, let it
    /// live for a while, hurt what it touches - and that skeleton is written HERE, once.
    /// A subclass only fills in the steps it actually cares about; it can never reorder
    /// them, forget the lifetime timer, or skip the damage call.
    ///
    /// It is also the pool's Product: it implements IPoolable, so it can reset itself and
    /// send itself home - without ever naming the pool that owns it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseProjectile : MonoBehaviour, IPoolable
    {
        [Tooltip("Colliders with this tag are ignored completely. The shooter stands inside " +
                 "his own muzzle, so without this the shot would end on the frame it starts.")]
        [SerializeField] private string ignoreTag = "Player";

        [Header("Scenery")]
        [Tooltip("Tick for a projectile that dies when it touches the ground or a wall " +
                 "(axe). Leave off for one that flies over the level (boomerang).")]
        [SerializeField] private bool stopsOnScenery;

        [Tooltip("Which layers count as scenery. Leave empty and ANY solid non-trigger " +
                 "collider stops it, which is what this project needs today.")]
        [SerializeField] private LayerMask blockingLayers;

        private ProjectileStats _stats;
        private Action _release;

        // Cached once in Awake. Looking a component up every shot is the classic
        // "GetComponent in the hot path" mistake - it is a lookup, not a field read.
        private Rigidbody2D _body;

        // Guards the two ways a shot can end at the same instant: hitting two enemies in
        // one physics step, or being hit at the exact frame its lifetime runs out.
        private bool _isLive;

        public ProjectileStats Stats { get { return _stats; } }

        protected Rigidbody2D Body { get { return _body; } }

        /// <summary>Filled in by the builder while the object is being assembled.</summary>
        public void Configure(ProjectileStats stats)
        {
            _stats = stats;
        }

        /// <summary>
        /// Handed over by the pool at creation time. The projectile learns HOW to go home,
        /// never WHERE home is, so it stays usable with any pool (Dependency Inversion).
        /// </summary>
        public void SetReleaseCallback(Action release)
        {
            _release = release;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        // ================= TEMPLATE METHOD =================
        /// <summary>
        /// The skeleton of a shot. The order is fixed and subclasses cannot change it -
        /// they only decide what the individual steps do.
        /// </summary>
        public void Fire(Vector3 origin)
        {
            transform.SetPositionAndRotation(origin, GetRotation());

            OnBeforeFire();                     // hook
            ApplyMovement(GetDirection());      // step
            OnAfterFire();                      // hook
        }
        // ==================================================

        /// <summary>The only step a projectile MUST answer: which way do I fly?</summary>
        protected abstract Vector2 GetDirection();

        /// <summary>How the projectile is turned when it appears. Default: not at all.</summary>
        protected virtual Quaternion GetRotation()
        {
            return Quaternion.identity;
        }

        /// <summary>
        /// Speed is set ONCE, here, and physics carries the object from then on. There is
        /// deliberately no Update: a per-frame position update for every projectile is
        /// exactly the cost this design is avoiding.
        /// </summary>
        protected virtual void ApplyMovement(Vector2 direction)
        {
            if (_body != null)
                _body.linearVelocity = direction.normalized * _stats.Speed;
        }

        protected virtual void OnBeforeFire() { }

        protected virtual void OnAfterFire() { }

        /// <summary>
        /// Non-damageable things that stop the flight - ground, ceiling, walls.
        ///
        /// The rule is DATA, not code: an axe that dies on the floor and a boomerang that
        /// flies over it are the same class with a different tick box. This used to be an
        /// override inside one projectile class, which meant every new projectile that wanted
        /// the same behaviour had to copy the same eight lines (Open/Closed).
        ///
        /// Still virtual: a projectile with a genuinely different rule - one that bounces,
        /// one that only stops on breakable walls - overrides it and loses nothing.
        /// </summary>
        protected virtual bool IsBlockedBy(Collider2D other)
        {
            if (!stopsOnScenery)
                return false;

            // Triggers are pickups, checkpoints and other projectiles - fly through those.
            if (other.isTrigger)
                return false;

            // No mask configured: treat every solid collider as a wall. Once the project
            // has a Ground layer, set the mask and the projectile stops only there -
            // fewer checks, same behaviour.
            if (blockingLayers.value == 0)
                return true;

            return (blockingLayers.value & (1 << other.gameObject.layer)) != 0;
        }

        /// <summary>
        /// The shared hit rule, written once: hurt whatever can be hurt through the SAME
        /// IDamageable every weapon uses, then leave unless this
        /// projectile is configured to pierce.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLive || other == null)
                return;

            // CompareTag, not other.tag == "Player": comparing the property allocates a
            // managed string on every single contact.
            if (!string.IsNullOrEmpty(ignoreTag) && other.CompareTag(ignoreTag))
                return;

            if (TryHit(other))
            {
                if (!_stats.PiercesEnemies)
                    Despawn();

                return;
            }

            if (IsBlockedBy(other))
                Despawn();
        }

        /// <summary>
        /// What this projectile does to what it touched, and whether that counted as a hit.
        ///
        /// The default is the game's shared rule: hurt anything that can be hurt. It is a
        /// HOOK rather than fixed code because the enemies shoot too, and their shots do
        /// the opposite - they ignore IDamageable and kill the player instead. That one
        /// difference is a single override, not a second projectile class (Open/Closed).
        ///
        /// The step order around it - ignore tag, hit, pierce or die, else check scenery -
        /// stays fixed, so no subclass can forget the lifetime or the pool handshake.
        /// </summary>
        /// <returns>True if something was hit, which is what ends the flight.</returns>
        protected virtual bool TryHit(Collider2D other)
        {
            // TryGetComponent instead of GetComponent + null check: it does not allocate
            // when nothing is found, and "nothing is found" is the common case here.
            IDamageable target;
            if (!other.TryGetComponent(out target))
                return false;

            // Bounced off - the ghost, or an axe against a stone. That ends even a piercing
            // flight: piercing means passing through what the shot HURT, not through armour.
            // Despawn is safe to call twice, so the caller's own Despawn is then a no-op.
            if (!target.TakeDamage(Stats.Damage))
                Despawn();

            return true;
        }

        /// <summary>Ends the flight and returns the object to whoever handed it out.</summary>
        public void Despawn()
        {
            if (!_isLive)
                return;

            _isLive = false;

            if (_release != null)
                _release();
            else
                gameObject.SetActive(false);    // no pool behind us: at least stop existing
        }

        /// <summary>
        /// Called by the pool the moment the object is handed out. A reused object must
        /// look exactly like a fresh one, which is what the old pool in the lecture got
        /// wrong: leftover velocity from the previous shot came back with it.
        /// </summary>
        public virtual void OnSpawned()
        {
            _isLive = true;

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.angularVelocity = 0f;
            }

            // Lifetime WITHOUT an Update. Every projectile running its own countdown per
            // frame means one engine call per projectile per frame, for a number that only
            // matters once. Invoke asks the engine to call us a single time instead.
            if (_stats.Lifetime > 0f)
                Invoke(nameof(ExpireByLifetime), _stats.Lifetime);
        }

        /// <summary>Called by the pool just before the object goes back to sleep.</summary>
        public virtual void OnDespawned()
        {
            _isLive = false;
            CancelInvoke();

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.angularVelocity = 0f;
            }
        }

        private void ExpireByLifetime()
        {
            Despawn();
        }
    }
}
