/// <summary>
/// A weapon that has to be FOUND before it works - the axe and the boomerang, as opposed
/// to something the player is simply born with.
///
/// Attack is the trigger. IsEquipped is the read side of the same state Equip/UnEquip
/// write. It is here so that the weapon slot can honour a weapon ticked "Unlocked From
/// Start" in the Inspector without ever naming BaseWeapon: it asks the interface who is
/// already carried and takes that one into the hand (Dependency Inversion).
/// </summary>
public interface IUseableWeapon
{
    /// <summary>Pull the trigger. Does nothing while the weapon is not in the player's hand.</summary>
    void Attack();

    /// <summary>True while this weapon is the one in the player's hand.</summary>
    bool IsEquipped { get; }

    void Equip();
    void UnEquip();
}
