using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// TEMPLATE METHOD, second application. Pulling the trigger always means the same
    /// three questions - am I unlocked, is the cooldown over, then shoot - and that order
    /// is written HERE. A weapon subclass supplies only the shooting.
    ///
    /// Because the "am I unlocked" gate lives in the base class, no weapon can forget it:
    /// the rule that Mario must find the power-up first cannot be skipped by a subclass
    /// that simply does not implement the check.
    ///
    /// It implements the project's existing IUseableWeapon (Equip/UnEquip), which is how
    /// the fire flower already unlocks the fireball - the laser reuses that mechanism
    /// instead of inventing a parallel one.
    ///
    /// (Not to be confused with Player/Liskov/BaseWeapon.cs, the plain-C# demo class from
    /// an earlier exercise. This one lives in the Game.Weapons namespace and is untouched
    /// by it.)
    /// </summary>
    public abstract class BaseWeapon : MonoBehaviour, IUseableWeapon
    {
        [Tooltip("Seconds between two shots")]
        [SerializeField] private float cooldown = 0.25f;

        [Tooltip("Tick only for weapons Mario owns from the start")]
        [SerializeField] private bool unlockedFromStart;

        // NegativeInfinity, not 0: at Time.time == 0 a zero would still be "one cooldown
        // ago", but only by luck. This says "has never fired" without relying on that.
        private float _lastFireTime = float.NegativeInfinity;

        private bool _isEquipped;
        private string _logPrefix;

        /// <summary>Which weapon this is. Every subclass must answer.</summary>
        public abstract WeaponType Type { get; }

        /// <summary>True once the matching power-up has been collected.</summary>
        public bool IsEquipped { get { return _isEquipped; } }

        /// <summary>Unlocked and off cooldown.</summary>
        public virtual bool CanFire
        {
            get { return _isEquipped && Time.time >= _lastFireTime + Cooldown; }
        }

        protected virtual float Cooldown { get { return cooldown; } }

        /// <summary>Built once and cached - "[Laser]", "[Fireball]"...</summary>
        protected string LogPrefix
        {
            get
            {
                if (_logPrefix == null)
                    _logPrefix = "[" + Type + "]";

                return _logPrefix;
            }
        }

        /// <summary>What the console says when Mario fires a weapon he has not found yet.</summary>
        protected virtual string LockedMessage
        {
            get { return LogPrefix + " Locked - pick up the " + Type + "PowerUp first"; }
        }

        // Private on purpose: a subclass that declared its own Awake would silently replace
        // this one and never get equipped. Subclasses use OnAwake() instead.
        private void Awake()
        {
            _isEquipped = unlockedFromStart;
            OnAwake();
        }

        /// <summary>Subclass setup. Cache references here, never in Attack().</summary>
        protected virtual void OnAwake() { }

        // ================= TEMPLATE METHOD =================
        /// <summary>
        /// The trigger. Fixed order, and a subclass cannot change it - it only fills in
        /// FireInternal().
        /// </summary>
        public void Attack()
        {
            if (!_isEquipped)
            {
                Debug.Log(LockedMessage);
                return;
            }

            if (!CanFire)
                return;

            // Only a shot that really left the barrel starts the cooldown. Otherwise a
            // weapon that could not fire - empty pool, no ammo - would still be punished
            // with the full wait, and the player would be blocked for a reason that never
            // happened.
            if (FireInternal())
                _lastFireTime = Time.time;
        }
        // ==================================================

        /// <summary>
        /// The one step every weapon defines for itself: actually shoot.
        /// </summary>
        /// <returns>True if a projectile was really fired.</returns>
        protected abstract bool FireInternal();

        public void Equip()
        {
            _isEquipped = true;
        }

        public void UnEquip()
        {
            _isEquipped = false;
        }
    }
}
