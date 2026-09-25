using UnityEngine;

/// <summary>
/// ROLE: Game rule: dying costs you the weapon.
/// PATTERNS: Observer-style - an IPlayerDeathHandler.
/// SOLID: O - a rule added as a component; D - empties IWeaponSlot.
///
/// One rule of the game, written once: dying costs you the weapon you found.
///
/// It empties the player's IWeaponSlot, so it locks the axe, the boomerang and anything
/// added tomorrow without naming a single weapon (Open/Closed, Dependency Inversion).
/// It used to sweep every IUseableWeapon on the player and un-equip each one; with a
/// single slot that sweep became both unnecessary and wrong - it would have cleared the
/// flags while leaving the slot pointing at a weapon the player no longer owns.
///
/// It is a SEPARATE component on purpose. The alternative - the slot, or each weapon,
/// being a death handler itself - would make "how firing works" depend on "how dying
/// works", which is not its business (Single Responsibility).
/// </summary>
[DisallowMultipleComponent]
public class WeaponsLostOnDeath : MonoBehaviour, IPlayerDeathHandler
{
    [Tooltip("Where to look for the weapon slot. Empty = this object and its children.")]
    [SerializeField] private Transform weaponsRoot;

    // Found once. Doing it on every death would be a GetComponentInChildren at the worst
    // possible moment, and the player's slot never moves.
    private IWeaponSlot _slot;

    private void Awake()
    {
        Transform root = weaponsRoot != null ? weaponsRoot : transform;

        // true = include inactive, so a slot on a switched-off child is still found.
        _slot = root.GetComponentInChildren<IWeaponSlot>(true);

        if (_slot == null)
            Debug.LogWarning("[Death] No IWeaponSlot under " + root.name +
                             " - weapons will survive death.", this);
    }

    /// <summary>Called by PlayerDeath once he is back at the start: empty hands.</summary>
    public void OnPlayerDied()
    {
        if (_slot == null)
            return;

        _slot.Clear();
    }
}
