/// <summary>
/// The one animal the player is riding - his saddle, exactly as IWeaponSlot is his hand.
///
/// Riding a second animal replaces the first, and that rule is the slot's to keep, so no
/// pickup has to remember to remove the previous one. It is a SEPARATE slot from the weapon
/// on purpose: finding an animal must not cost the player his axe, and stepping off it must
/// give the axe straight back - two independent truths, two slots (Single Responsibility).
///
/// Two methods, because there are exactly two things the rest of the game does to the
/// saddle: a pickup puts an animal in it, and being hit or dying empties it. Neither caller
/// learns which animals exist (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IMountSlot
{
    /// <summary>Climb onto this animal, leaving whatever was carried before.</summary>
    void Mount(AnimalMount animal);

    /// <summary>Back on foot.</summary>
    void Dismount();
}
