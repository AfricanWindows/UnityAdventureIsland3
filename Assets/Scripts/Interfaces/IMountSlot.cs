/// <summary>
/// The one animal the player is riding - his saddle, exactly as IWeaponSlot is his hand.
///
/// Riding a second animal replaces the first, and that rule is the slot's to keep, so no
/// pickup has to remember to remove the previous one. It is a SEPARATE slot from the weapon
/// on purpose: finding an animal must not cost the player his axe, and stepping off it must
/// give the axe straight back - two independent truths, two slots (Single Responsibility).
///
/// Three members, because there are exactly three things the rest of the game does with the
/// saddle: a pickup puts an animal in it, the hit absorber asks whether one is there, and
/// being hit or dying empties it. No caller learns which animals exist (Interface
/// Segregation, Dependency Inversion).
/// </summary>
public interface IMountSlot
{
    /// <summary>True while the player rides an animal.</summary>
    bool IsMounted { get; }

    /// <summary>Climb onto this animal, leaving whatever was carried before.</summary>
    void Mount(AnimalMount animal);

    /// <summary>Back on foot.</summary>
    void Dismount();
}
