using System;
using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// ROLE: Base of every weapon: Attack = unlocked? cooldown over? then fire.
    /// PATTERNS: Template Method - Attack is fixed, FireInternal is abstract; Observer - raises Attacked.
    /// SOLID: D - the rest of the game sees only IUseableWeapon.
    ///
    /// TEMPLATE METHOD, second application. Pulling the trigger always means the same
    /// three questions - am I unlocked, is the cooldown over, then shoot - and that order
    /// is written HERE. A weapon subclass supplies only the shooting.
    ///
    /// Because the "am I unlocked" gate lives in the base class, no weapon can forget it:
    /// the rule that the player must find the power-up first cannot be skipped by a subclass
    /// that simply does not implement the check.
    ///
    /// It implements IUseableWeapon, so the weapon slot and the pick-ups work with any weapon
    /// without ever naming this class (Dependency Inversion).
    /// </summary>
    public abstract class BaseWeapon : MonoBehaviour, IUseableWeapon, IResettable, IAttacker
    {
        /// <summary>
        /// Raised on the frame a shot really leaves - the same announcement the shooting
        /// snake already makes, through the same interface.
        ///
        /// It is here and not in each weapon because the moment is decided here: only a shot
        /// that passed the equipped gate, the cooldown and its own FireInternal counts. A
        /// listener therefore never animates a trigger pull that did nothing.
        ///
        /// The weapon does not know who listens. PlayerAnimatorView does today, a sound or a
        /// muzzle flash might tomorrow, and none of that belongs to the thing that decides
        /// WHEN to shoot (Single Responsibility, Open/Closed).
        /// </summary>
        public event Action Attacked;

        [Tooltip("Seconds between two shots")]
        [SerializeField] private float cooldown = 0.25f;

        [Tooltip("Tick only for weapons the player owns from the start")]
        [SerializeField] private bool unlockedFromStart;

        // NegativeInfinity, not 0: at Time.time == 0 a zero would still be "one cooldown
        // ago", but only by luck. This says "has never fired" without relying on that.
        private float _lastFireTime = float.NegativeInfinity;

        private bool _isEquipped;
        private string _logPrefix;

        /// <summary>
        /// The Inspector flag, read straight from the serialized field. Deserialization
        /// happens before any Awake, so this answer is settled before the first line of game
        /// code runs - which is what removed the start-up race the weapon slot used to work
        /// around by reading the equipped flag in Start instead of Awake.
        /// </summary>
        public bool IsOwnedFromStart { get { return unlockedFromStart; } }

        /// <summary>
        /// Unlocked and off cooldown. A hook of the Attack() template: a weapon with a rule
        /// of its own - the boomerang, which waits until it is home - adds it here.
        ///
        /// Protected, not public: Attack() is the only one that asks. The weapon slot never
        /// needs to, because pulling the trigger of a weapon that cannot fire simply does
        /// nothing - so nothing outside the weapon depends on this question.
        /// </summary>
        protected virtual bool CanFire
        {
            get { return _isEquipped && Time.time >= _lastFireTime + cooldown; }
        }

        /// <summary>
        /// Built once and cached - "[AxeWeapon]", "[BoomerangWeapon]". The class name, so a new
        /// weapon needs no name of its own to be told apart in the console.
        /// </summary>
        protected string LogPrefix
        {
            get
            {
                if (_logPrefix == null)
                    _logPrefix = "[" + GetType().Name + "]";

                return _logPrefix;
            }
        }

        // Private on purpose: a subclass that declared its own Awake would silently replace
        // this one. Subclasses use OnAwake() instead.
        //
        // It no longer pre-sets the equipped flag from unlockedFromStart. That line was what
        // gave the flag two meanings at once - "in his hand" and "his from the start" - and
        // the weapon slot used to read the second one out of it. The slot now asks
        // IsOwnedFromStart and calls Equip() itself, so the flag means one thing only.
        private void Awake()
        {
            OnAwake();
        }

        /// <summary>
        /// Subclass setup. Cache references here, never in Attack(). Abstract, not an empty
        /// virtual: every weapon has setup of its own, so an empty default would never run.
        /// </summary>
        protected abstract void OnAwake();

        // ================= TEMPLATE METHOD =================
        /// <summary>
        /// The trigger. Fixed order, and a subclass cannot change it - it only fills in
        /// FireInternal().
        /// </summary>
        public void Attack()
        {
            if (!_isEquipped)
                return;

            if (!CanFire)
                return;

            // Only a shot that really left the barrel starts the cooldown. Otherwise a
            // weapon that could not fire - empty pool, no ammo - would still be punished
            // with the full wait, and the player would be blocked for a reason that never
            // happened.
            if (!FireInternal())
                return;

            _lastFireTime = Time.time;

            if (Attacked != null)
                Attacked();
        }
        // ==================================================

        /// <summary>
        /// The one step every weapon defines for itself: actually shoot.
        /// </summary>
        /// <returns>True if a projectile was really fired.</returns>
        protected abstract bool FireInternal();

        /// <summary>
        /// A new game gives back exactly the weapon the player started with - the one
        /// ticked Unlocked From Start - and takes away anything he found.
        ///
        /// This is the only writer of the equipped flag besides the weapon slot, and the
        /// two cannot disagree: the slot restores the weapon it remembered at start-up
        /// rather than reading these flags, so a restart lands in the same state whichever
        /// of the two IResettables it happens to call first.
        /// </summary>
        public void ResetToStart()
        {
            _isEquipped = unlockedFromStart;
        }

        /// <summary>
        /// Put in the player's hand. Called by whichever SLOT is holding this weapon and by
        /// nothing else: the weapon slot for the axe and the boomerang, the saddle for an
        /// animal's attack. Keeping to one at a time is the slot's rule, and anyone else
        /// writing this flag would be a second opinion about what the player is carrying.
        /// </summary>
        public void Equip()
        {
            _isEquipped = true;
        }

        /// <summary>Taken away - by a swap, or by dying. The slot decides, not the weapon.</summary>
        public void UnEquip()
        {
            _isEquipped = false;
        }
    }
}
