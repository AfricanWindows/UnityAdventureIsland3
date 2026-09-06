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
    ///
    /// (This is the pooled hierarchy added in exercise 3. The older global BaseProjectile,
    /// used by the fireball and the axe, is untouched and keeps working as before.)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseProjectile : MonoBehaviour, IPoolable
    {
        [Tooltip("Colliders with this tag are ignored completely. The shooter stands inside " +
                 "his own muzzle, so without this the shot would end on the frame it starts.")]
        [SerializeField] private string ignoreTag = "Player";

        private ProjectileStats _stats;
        private Action _release;

        // Cached once in Awake. Looking a component up every shot is the classic
        // "GetComponent in the hot path" mistake - it is a lookup, not a field read.
        private Rigidbody2D _body;

        // Guards the two ways a laser can end at the same instant: hitting two enemies in
        // one physics step, or being hit at the exact frame its lifetime runs out.
        private bool _isLive;

        public ProjectileStats Stats { get { return _stats; } }

        protected Rigidbody2D Body { get { return _body; } }

        /// <summary>Prefix for this projectile's console messages, e.g. "[Laser]".</summary>
        protected virtual string LogPrefix { get { return "[Projectile]"; } }

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

            Debug.Log(LogPrefix + " Fired from " + origin);
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
        /// Default: nothing stops it, so a projectile that ignores scenery needs no code.
        /// </summary>
        protected virtual bool IsBlockedBy(Collider2D other)
        {
            return false;
        }

        /// <summary>
        /// The shared hit rule, written once: hurt whatever can be hurt through the SAME
        /// IDamageable the fireball and the axe already use, then leave unless this
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

            // TryGetComponent instead of GetComponent + null check: it does not allocate
            // when nothing is found, and "nothing is found" is the common case here.
            IDamageable target;
            if (other.TryGetComponent(out target))
            {
                target.TakeDamage(_stats.Damage);
                Debug.Log(LogPrefix + " Hit " + other.name);

                if (!_stats.PiercesEnemies)
                    Despawn();

                return;
            }

            if (IsBlockedBy(other))
            {
                Debug.Log(LogPrefix + " Hit " + other.name);
                Despawn();
            }
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
            Debug.Log(LogPrefix + " Lifetime expired");
            Despawn();
        }
    }
}
