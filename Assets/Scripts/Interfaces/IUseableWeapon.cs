/// <summary>
/// ROLE: A weapon: attack, equip, unequip, owned from the start or not.
/// PATTERNS: none - a role interface.
/// SOLID: D - the slot and the pickups never name a weapon class.
///
/// A weapon that has to be FOUND before it works - the axe and the boomerang, as opposed
/// to something the player is simply born with.
///
/// Attack is the trigger, Equip/UnEquip are the slot putting it in his hand and taking it
/// out, and IsOwnedFromStart says whether he begins the game already carrying it.
///
/// It used to carry IsEquipped as well, and the slot read THAT at start-up to mean "owned
/// from the start" - true only because BaseWeapon.Awake happened to copy the Inspector flag
/// into the equipped flag. Any other implementation would have obeyed the signature perfectly
/// and still broken the starting weapon, because the real rule was written nowhere (Liskov
/// Substitution). Asking IsOwnedFromStart says out loud what was being asked, and left
/// IsEquipped with no reader at all outside the weapon itself - so it is gone from here
/// too (Interface Segregation). Whether a weapon is in his hand is the SLOT's business, and
/// the slot already knows: it is the only thing that puts one there.
/// </summary>
public interface IUseableWeapon
{
    /// <summary>Pull the trigger. Does nothing while the weapon is not in the player's hand.</summary>
    void Attack();

    /// <summary>
    /// True if the player begins the game carrying this one, before finding any pickup.
    /// Fixed by the designer in the Inspector, and never changed by play.
    /// </summary>
    bool IsOwnedFromStart { get; }

    void Equip();
    void UnEquip();
}
