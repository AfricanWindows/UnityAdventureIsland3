using Game.Core;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// The player's single weapon slot, and the trigger finger that fires it.
///
/// He carries ONE weapon, exactly as in the original game: finding the boomerang throws
/// the axe away. That rule is written in Equip() and nowhere else, which is the whole
/// point of this class - before it, "what is in my hand" lived as a bool on every weapon
/// AND as an index here, so picking up a second weapon left the first one equipped too,
/// and the trigger fired whichever weapon came first in the list rather than the one just
/// found.
///
/// It is the ONLY caller of Equip/UnEquip on a weapon. That is what makes the flags
/// impossible to desynchronise: there is one writer, so there is nothing to disagree with.
///
/// It does not know which weapons exist. A pickup hands it an IUseableWeapon and it takes
/// it; a third weapon added tomorrow needs no edit here (Open/Closed, Dependency
/// Inversion). It does not know about dying either - WeaponsLostOnDeath calls Clear().
///
/// Slot selection with the number keys is gone along with the list: with one hand there is
/// nothing to choose between, and IInputSource lost the two members that served it.
/// </summary>
public class WeaponsHandler : InputDrivenBehaviour, IWeaponSlot, IResettable
{
    [Tooltip("Where to look for weapons. Empty = this object's parent (the player).")]
    [SerializeField] private Transform weaponsRoot;

    [Tooltip("Log every swap. Handy while building levels, noise in a finished game.")]
    [SerializeField] private bool verbose = true;

    // The weapon in hand, or null for empty hands. Held as the interface, and compared
    // with a plain null check: these components live on the player and are never
    // destroyed while he exists, so Unity's "destroyed object pretends to be null" trick
    // - which an interface-typed field would NOT reproduce - cannot bite here.
    private IUseableWeapon _current;

    // What the player began the game holding, decided once in Start. Captured before any
    // pickup can happen, so a restart never has to guess (see ResetToStart).
    private IUseableWeapon _startingWeapon;

    // Everything that may hold the trigger shut. Collected once, through the interface, so
    // this class never learns that animals exist (Dependency Inversion) - the same shape
    // PlayerMovement already uses for IMovementLock.
    private IAttackLock[] _attackLocks;

    /// <summary>
    /// Start, not Awake: a weapon's own Awake is what turns "Unlocked From Start" into
    /// IsEquipped, and Unity gives no order between the Awakes of two different objects.
    /// Every Awake has run by the time any Start does, so the scan below reads a settled
    /// answer instead of a race.
    /// </summary>
    private void Start()
    {
        TakeStartingWeapon();

        // true = include inactive, so a lock on a switched-off component still counts.
        _attackLocks = ResolveRoot().GetComponentsInChildren<IAttackLock>(true);
    }

    private void Update()
    {
        if (!HasInput)
            return;

        if (!InputSource.AttackPressed)
            return;

        // Somebody else has the trigger - riding an animal is the one case today, and the
        // same button reaches the animal instead. The weapon is NOT taken away, it simply
        // waits, so stepping off gives it straight back with nothing to restore.
        if (IsAttackBlocked())
            return;

        if (_current == null)
        {
            if (verbose)
                Debug.Log("[Weapons] Nothing to throw - no weapon picked up yet");

            return;
        }

        // Whether the shot is allowed - cooldown, a boomerang still in the air - is the
        // weapon's own business, answered behind Attack() (see BaseWeapon).
        _current.Attack();
    }

    // ===================== IWeaponSlot =====================

    /// <summary>
    /// The override the game is built on: the old weapon is dropped, the new one is held.
    /// </summary>
    public void Equip(IUseableWeapon weapon)
    {
        if (weapon == null)
            return;

        // ReferenceEquals, not ==: both sides are interfaces, and this says plainly that
        // identity is what is being asked. Walking over the same pickup twice, or a
        // restart that hands back the weapon already held, must not un-equip and re-equip
        // the same object - a weapon is entitled to treat UnEquip as "you lost me".
        if (ReferenceEquals(weapon, _current))
        {
            if (verbose)
                Debug.Log("[Weapons] Already carrying " + Name(weapon));

            return;
        }

        if (_current != null)
        {
            _current.UnEquip();

            if (verbose)
                Debug.Log("[Weapons] Dropped " + Name(_current));
        }

        _current = weapon;
        _current.Equip();

        if (verbose)
            Debug.Log("[Weapons] Now carrying " + Name(_current));
    }

    /// <summary>Empty hands - what dying costs him.</summary>
    public void Clear()
    {
        if (_current == null)
            return;

        _current.UnEquip();

        if (verbose)
            Debug.Log("[Weapons] Lost " + Name(_current));

        _current = null;
    }

    // =======================================================

    /// <summary>
    /// A new game gives back exactly what the player started with - usually nothing.
    ///
    /// It reads _startingWeapon rather than the weapons' own flags, so it does not care
    /// whether BaseWeapon.ResetToStart happened before or after this call. The restart
    /// walks every IResettable in an order nobody controls, and this is what keeps the
    /// slot and the flags agreeing whichever way that walk goes.
    /// </summary>
    public void ResetToStart()
    {
        if (_startingWeapon != null)
            Equip(_startingWeapon);
        else
            Clear();
    }

    /// <summary>
    /// Puts into the hand whatever weapon says it is already equipped - that is, one
    /// ticked "Unlocked From Start". Today no weapon is, and the player starts empty
    /// handed; the scan costs one search at startup and keeps that Inspector flag honest
    /// instead of quietly meaningless.
    /// </summary>
    private void TakeStartingWeapon()
    {
        Transform root = ResolveRoot();

        // true = include inactive, so a weapon sitting on a switched-off child counts.
        IUseableWeapon[] found = root.GetComponentsInChildren<IUseableWeapon>(true);

        for (int i = 0; i < found.Length; i++)
        {
            if (!found[i].IsEquipped)
                continue;

            if (_startingWeapon == null)
            {
                _startingWeapon = found[i];
                continue;
            }

            // One hand, so a second one cannot be carried. Said out loud, because this is
            // a mistake in the Inspector rather than in the game.
            Debug.LogWarning("[Weapons] " + Name(found[i]) + " is also ticked Unlocked From " +
                             "Start, but the player has one slot - keeping " +
                             Name(_startingWeapon) + ".", this);

            found[i].UnEquip();
        }

        if (_startingWeapon != null)
            Equip(_startingWeapon);
    }

    /// <summary>
    /// Where the player's weapons and his locks are looked for. This component sits on a
    /// child object, so the search starts at the player himself unless told otherwise.
    /// </summary>
    private Transform ResolveRoot()
    {
        if (weaponsRoot != null)
            return weaponsRoot;

        return transform.parent != null ? transform.parent : transform;
    }

    private bool IsAttackBlocked()
    {
        if (_attackLocks == null)
            return false;

        for (int i = 0; i < _attackLocks.Length; i++)
        {
            if (_attackLocks[i].BlocksAttack)
                return true;
        }

        return false;
    }

    private static string Name(IUseableWeapon weapon)
    {
        return weapon != null ? weapon.GetType().Name : "nothing";
    }
}
