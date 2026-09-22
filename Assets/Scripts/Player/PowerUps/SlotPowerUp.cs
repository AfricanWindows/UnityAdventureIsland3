using UnityEngine;

/// <summary>
/// "Find item TItem on the player and put it into his TSlot" - the one shape shared by the
/// weapons (IWeaponSlot) and the animals (IMountSlot).
///
/// EquipWeaponPowerUp and MountAnimalPowerUp were twins: both searched the player for an
/// item, complained if it was missing, and handed it to a slot. The search and the complaint
/// live here now; each twin only says which slot method takes the item (Template Method,
/// Don't Repeat Yourself).
///
/// The item goes into the SLOT, never straight onto the player. The slot is the one place
/// that knows the player holds one weapon and rides one animal at a time, so no power-up has
/// to remember to take the previous one away - and cannot forget (Single Responsibility).
/// A player WITHOUT the slot gets an error and nothing else - deliberately no fallback to
/// equipping the item directly: nothing would pull its trigger, and it would quietly bring
/// back the two-at-once bug the slot exists to prevent.
/// </summary>
/// <typeparam name="TSlot">The interface of the slot the item goes into.</typeparam>
/// <typeparam name="TItem">The component to find on the player and put into the slot.</typeparam>
public abstract class SlotPowerUp<TSlot, TItem> : PlayerComponentPowerUp<TSlot>
    where TSlot : class
    where TItem : Component
{
    protected sealed override void Apply(TSlot slot, GameObject player)
    {
        // Searches inactive children too, so the item may sit on the player switched off.
        TItem item = player.GetComponentInChildren<TItem>(true);

        if (item == null)
        {
            Debug.LogError("[PowerUp] No " + typeof(TItem).Name + " under " + player.name +
                           " - add the component to the player prefab.", player);
            return;
        }

        PutIn(slot, item);
    }

    /// <summary>The one step each slot power-up writes: which slot method takes the item.</summary>
    protected abstract void PutIn(TSlot slot, TItem item);
}
