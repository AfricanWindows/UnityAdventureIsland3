using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Projectiles
{
    /// <summary>
    /// CONCRETE PRODUCT - the same slot LaserProjectile fills for its weapon.
    ///
    /// It rides one oval loop the way Mario faces and comes back to his hand. The loop
    /// also tracks Mario's CURRENT position, so it lands on him even if he kept running
    /// or jumped.
    ///
    /// It reuses the whole pooled skeleton from BaseProjectile (the Fire() template, the
    /// lifetime timer, the shared IDamageable hit rule, the pool handshake) and only
    /// replaces one inherited assumption - that a projectile is pushed once and carried by
    /// physics - with a hand-driven out-and-back path, a step per FixedUpdate.
    ///
    /// The trip lasts exactly Stats.Lifetime seconds; when the base class's lifetime timer
    /// fires, the boomerang returns itself to the pool. Keep Pierces Enemies ON in the
    /// config asset so a mid-air hit never cuts the trip short.
    /// </summary>
    public sealed class BoomerangProjectile : BaseProjectile
    {
        [FormerlySerializedAs("radius")]
        [Tooltip("Oval WIDTH - how far in front of Mario it reaches, in world units. " +
                 "Keep this bigger than the height for a horizontal oval.")]
        [SerializeField] private float throwDistance = 6f;

        [Tooltip("Oval HEIGHT - total vertical size (outbound branch dips down by half, " +
                 "return branch rises up by half), in world units.")]
        [SerializeField] private float returnArcHeight = 2f;

        [Tooltip("Sprite spin while flying, degrees per second")]
        [SerializeField] private float spinDegreesPerSecond = 720f;

        protected override string LogPrefix { get { return "[Boomerang]"; } }

        private Transform _owner;
        private float _facing = 1f;
        private Vector2 _start;
        private float _elapsed;
        private float _spinAngle;
        private bool _isFlying;

        /// <summary>True from launch until it is back in the pool. The weapon reads this to
        /// keep to one boomerang at a time.</summary>
        public bool IsFlying { get { return _isFlying; } }

        /// <summary>
        /// The weapon's single call. Records the hand that threw it - so the return leg can
        /// follow Mario as he moves - then hands over to the inherited Fire() template.
        /// </summary>
        public void Throw(Vector3 origin, Transform owner, float facing)
        {
            _owner = owner;
            _facing = facing >= 0f ? 1f : -1f;
            Fire(origin);
        }

        // Motion is a hand-driven out-and-back, not a straight velocity push.
        protected override Vector2 GetDirection() { return Vector2.zero; }

        protected override void ApplyMovement(Vector2 direction) { }

        /// <summary>Runs at the end of Fire(): the object is already sitting at the origin.</summary>
        protected override void OnAfterFire()
        {
            _start = transform.position;
            _elapsed = 0f;
            _spinAngle = transform.eulerAngles.z;
            _isFlying = true;
        }

        private void FixedUpdate()
        {
            if (!_isFlying)
                return;

            float duration = Stats.Lifetime > 0f ? Stats.Lifetime : 1f;
            _elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_elapsed / duration);

            Vector2 local = LoopOffset(t);

            // Slide the whole loop onto where Mario's hand is NOW, fully by t = 1, so the
            // catch lands on him wherever he ran or jumped to.
            Vector2 ownerNow = _owner != null ? (Vector2)_owner.position : _start;
            Vector2 next = _start + local + Vector2.Lerp(Vector2.zero, ownerNow - _start, t);

            _spinAngle += spinDegreesPerSecond * Time.fixedDeltaTime * _facing;

            if (Body != null)
            {
                Body.MovePosition(next);
                Body.MoveRotation(_spinAngle);
            }
            else
            {
                transform.SetPositionAndRotation(next, Quaternion.Euler(0f, 0f, _spinAngle));
            }

            // Back home. Despawn here too, in case the base lifetime timer drifts by a
            // frame - Despawn() is guarded, so calling it twice is harmless.
            if (t >= 1f)
            {
                Debug.Log("[Boomerang] Back to Mario - returned to pool");
                Despawn();
            }
        }

        /// <summary>
        /// A plain oval, relative to the throw point and (0,0) at t = 0 and t = 1.
        /// X sweeps forward to throwDistance and back; Y dips down half the height on the
        /// way out and rises up half the height on the way back. Width &gt; height gives a
        /// horizontal oval.
        /// </summary>
        private Vector2 LoopOffset(float t)
        {
            float angle = t * Mathf.PI * 2f;

            float x = _facing * throwDistance * 0.5f * (1f - Mathf.Cos(angle));
            float y = -returnArcHeight * 0.5f * Mathf.Sin(angle);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Always called by the pool on release, whatever ended the trip. The single place
        /// _isFlying is cleared, so the weapon can never be left thinking a destroyed
        /// boomerang is still out.
        /// </summary>
        public override void OnDespawned()
        {
            _isFlying = false;
            _owner = null;
            _elapsed = 0f;
            base.OnDespawned();
        }
    }
}
