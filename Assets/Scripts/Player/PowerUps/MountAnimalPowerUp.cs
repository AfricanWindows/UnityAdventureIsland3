/// <summary>
/// "Picking this up puts the player on animal T, and whatever he was riding is gone."
///
/// One class for all three animals, and for any animal added later. It is the twin of
/// EquipWeaponPowerUp, and both are now written on the same base - SlotPowerUp finds the
/// item and the slot, this class only says "the animal goes into IMountSlot.Mount".
///
/// This is where the generic genuinely pays: THREE closed types today, each named by a
/// three-line pickable, and a fourth animal needs no new effect at all (Open/Closed).
///
/// The constraint is AnimalMount, not a concrete animal: this never learns what a frog or a
/// lizard is, only that whatever it found can be ridden (Dependency Inversion).
/// </summary>
/// <typeparam name="TMount">The animal component to put the player on.</typeparam>
public class MountAnimalPowerUp<TMount> : SlotPowerUp<IMountSlot, TMount>
    where TMount : AnimalMount
{
    protected override void PutIn(IMountSlot slot, TMount animal)
    {
        slot.Mount(animal);
    }
}
