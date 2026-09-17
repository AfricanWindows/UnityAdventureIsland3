using UnityEngine;

/// <summary>
/// "Picking this up puts the player on animal T, and whatever he was riding is gone."
///
/// One class for all three animals, and for any animal added later. It is the exact twin of
/// EquipWeaponPowerUp, which replaced four near-identical weapon power-ups that differed
/// only in a type name - the same three classes would have grown here otherwise, and a fix
/// to one of them would have reached one animal and silently missed the other two.
///
/// This is where the generic genuinely pays: THREE closed types today, each named by a
/// three-line pickable, and a fourth animal needs no new effect at all (Open/Closed).
///
/// It hands the animal to the player's IMountSlot instead of climbing on itself. That single
/// redirection is what makes the swap work: the slot is the one place that knows a player
/// rides one animal at a time, so this class never has to remember to remove the previous
/// one - and cannot forget (Single Responsibility).
///
/// The constraint is AnimalMount, not a concrete animal: this never learns what a frog or a
/// lizard is, only that whatever it found can be ridden (Dependency Inversion).
/// </summary>
/// <typeparam name="TMount">The animal component to put the player on.</typeparam>
public class MountAnimalPowerUp<TMount> : IPowerUp where TMount : AnimalMount
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        // Searches inactive children too, so an animal may sit on the player switched off.
        TMount animal = player.GetComponentInChildren<TMount>(true);

        if (animal == null)
        {
            Debug.LogWarning("[Mount] No " + typeof(TMount).Name + " under " + player.name +
                             " - add the component to the player prefab.", player);
            return;
        }

        IMountSlot slot = player.GetComponentInChildren<IMountSlot>(true);

        if (slot == null)
        {
            // Deliberately NOT falling back to riding the animal directly. A player with no
            // saddle has nothing that fires the animal's attack, so the fallback would put
            // him on an animal that can never attack while quietly bringing back the
            // two-animals-at-once bug the slot exists to prevent.
            Debug.LogError("[Mount] " + player.name + " has no IMountSlot - add the " +
                           "PlayerMount component. Animal not given.", player);
            return;
        }

        slot.Mount(animal);
    }
}
