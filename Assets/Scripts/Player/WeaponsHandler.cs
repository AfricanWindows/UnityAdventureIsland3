using Game.Core;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// ROLE: The player's weapon slot and the attack button.
/// PATTERNS: DI - the input arrives through InputDrivenBehaviour.
/// SOLID: O, D - fires an IUseableWeapon, never a concrete weapon.
///
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

    // The weapon in hand, or null for empty hands. Held as the interface, and compared
    // with a plain null check: these components live on the player and are never
    // destroyed while he exists, so Unity's "destroyed object pretends to be null" trick
    // - which an interface-typed field would NOT reproduce - cannot bite here.
    private IUseableWeapon _current;

    // What the player began the game holding, decided once in Start. Captured before any
    // pickup can happen, so a restart never has to guess (see ResetToStart).
    private IUseableWeapon _startingWeapon;

    // Everything that may take the button away from the weapon in his hand. Collected once,
    // through the interface, so this class never learns that animals exist (Dependency
    // Inversion) - the same shape PlayerMovement already uses for IMovementLock.
    private IAttackOverride[] _attackOverrides;

    /// <summary>
    /// Start, not Awake. It used to HAVE to be Start: the scan read IsEquipped, which a
    /// weapon's own Awake filled in from its Inspector flag, and Unity gives no order between
    /// the Awakes of two different objects. Now the scan asks IsOwnedFromStart, which is the
    /// serialized field itself and is settled before any Awake, so the race is gone. Start is
    /// kept because the locks below still want every Awake to have run.
    /// </summary>
    private void Start()
    {
        TakeStartingWeapon();

        // true = include inactive, so an override on a switched-off component still counts.
        _attackOverrides = ResolveRoot().GetComponentsInChildren<IAttackOverride>(true);
    }

    /// <summary>
    /// The ONE place in the game that reads the attack button. Riding an animal used to add
    /// a second reader on the player and a flag to keep the two from both firing; now the
    /// animal only answers WHICH weapon the button is wired to, and this stays the single
    /// trigger finger.
    /// </summary>
    private void Update()
    {
        if (!HasInput)
            return;

        if (!InputSource.AttackPressed)
            return;

        IUseableWeapon weapon = ResolveWeapon();

        // No weapon and no animal: the attack button does nothing, as the assignment says.
        if (weapon == null)
            return;

        // Whether the shot is allowed - cooldown, a boomerang still in the air - is the
        // weapon's own business, answered behind Attack() (see BaseWeapon).
        weapon.Attack();
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
            return;

        if (_current != null)
            _current.UnEquip();

        _current = weapon;
        _current.Equip();
    }

    /// <summary>Empty hands - what dying costs him.</summary>
    public void Clear()
    {
        if (_current == null)
            return;

        _current.UnEquip();
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
    /// Puts into the hand whatever weapon says the player owns from the start - one ticked
    /// "Unlocked From Start". Today no weapon is, and the player starts empty handed; the
    /// scan costs one search at startup and keeps that Inspector flag honest instead of
    /// quietly meaningless.
    ///
    /// It asks IsOwnedFromStart and not IsEquipped. Those were the same answer once, by
    /// accident of how BaseWeapon woke up, which meant any other IUseableWeapon would have
    /// silently lost its starting weapon here. The question being asked is now the question
    /// being answered.
    /// </summary>
    private void TakeStartingWeapon()
    {
        Transform root = ResolveRoot();

        // true = include inactive, so a weapon sitting on a switched-off child counts.
        IUseableWeapon[] found = root.GetComponentsInChildren<IUseableWeapon>(true);

        for (int i = 0; i < found.Length; i++)
        {
            if (!found[i].IsOwnedFromStart)
                continue;

            if (_startingWeapon == null)
            {
                _startingWeapon = found[i];
                continue;
            }

            // One hand, so a second one cannot be carried. Said out loud, because this is
            // a mistake in the Inspector rather than in the game. Nothing to un-equip: no
            // weapon is in the hand until the Equip below puts one there.
            Debug.LogWarning("[Weapons] " + Name(found[i]) + " is also ticked Unlocked From " +
                             "Start, but the player has one slot - keeping " +
                             Name(_startingWeapon) + ".", this);
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

    /// <summary>
    /// What the button fires this frame: whoever has claimed it, otherwise the weapon in
    /// his hand. The weapon is never taken AWAY while an animal has the button, it simply
    /// waits - so stepping off gives it straight back with nothing to restore.
    /// </summary>
    private IUseableWeapon ResolveWeapon()
    {
        if (_attackOverrides != null)
        {
            for (int i = 0; i < _attackOverrides.Length; i++)
            {
                IUseableWeapon claimed = _attackOverrides[i].OverrideWeapon;

                if (claimed != null)
                    return claimed;
            }
        }

        return _current;
    }

    private static string Name(IUseableWeapon weapon)
    {
        return weapon != null ? weapon.GetType().Name : "nothing";
    }
}
