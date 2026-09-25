/// <summary>
/// ROLE: The player's one hand: put a weapon in, empty it.
/// PATTERNS: none - a slot (one role over one state).
/// SOLID: S - "one weapon at a time" is kept by the slot.
///
/// The player's ONE pair of hands: whatever he carries, he carries INSTEAD of what he
/// carried before.
///
/// This interface exists because "which weapon is in hand" used to be stored twice - a
/// bool on every weapon plus an index in the handler - and two copies of one truth always
/// drift apart. Picking up the boomerang left the axe equipped as well, and the trigger
/// still fired whichever weapon happened to be first in the list.
///
/// Now the slot is the single owner of that truth, and Equip/UnEquip on a weapon are
/// written by nobody else. A weapon therefore cannot end up half-carried, and no caller
/// has to remember to take the old one away first - that IS the slot's job.
///
/// Two methods, because there are exactly two things the rest of the game does to the
/// slot: a pickup puts a weapon in it, and dying empties it (Interface Segregation).
/// Neither caller learns what a weapon is, how firing works, or which weapons exist
/// (Dependency Inversion).
/// </summary>
public interface IWeaponSlot
{
    /// <summary>
    /// Take this weapon into the hand, dropping whatever was there. Passing the weapon
    /// already held changes nothing.
    /// </summary>
    void Equip(IUseableWeapon weapon);

    /// <summary>Empty the hand. The player keeps nothing.</summary>
    void Clear();
}
