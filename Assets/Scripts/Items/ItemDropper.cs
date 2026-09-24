using System.Collections.Generic;
using Game.Core;
using UnityEngine;

/// <summary>
/// "When something asks me to, put one item into the world - either the one I was told to
/// hold, or one drawn at random from a list."
///
/// This is the whole of WHAT drops. It deliberately does not know WHEN: the egg breaks on
/// touch, and the assignment also wants a beaten enemy to leave an animal behind. Those are
/// two different triggers over the same job, so the trigger is a separate component that
/// calls DropItem() through IItemDropper (Single Responsibility, Open/Closed). Same split the project already uses
/// for PlayerJump + JumpBehaviour and HoppingEnemy + HopAim.
///
/// GENERIC over what it drops, and that is not decoration: the Inspector slots become typed.
/// A PickableDropper shows a BasePickable field, so a floor tile or an enemy CANNOT be
/// dragged into the loot list by mistake - the mistake is caught in the editor instead of at
/// runtime. A dropper for something that is not a pickable - a spawner that drops an enemy,
/// say - is one new subclass and no new code here.
///
/// The contents are PREFABS, never an enum of item kinds. That is why the three animals and
/// the fairy cost this class nothing at all: each one was dragged into the list on the egg
/// prefab, and nobody opened a script.
///
/// Nor is there an enum for HOW the item is chosen. There used to be one - Random or
/// Specific - and it only repeated what the data already says: a filled Specific Item slot
/// IS the choice "always this one", an empty one means "draw from the pool". One rule, read
/// off the Inspector, instead of a mode switch that every new way of choosing would have had
/// to edit (Open/Closed).
///
/// Note it is abstract: Unity cannot put an open generic MonoBehaviour on a GameObject, so
/// every dropper needs a concrete subclass - the same shape as ProjectilePoolManager and
/// AxePoolManager.
/// </summary>
/// <typeparam name="TItem">What this dropper is allowed to drop.</typeparam>
public abstract class ItemDropper<TItem> : MonoBehaviour, IResettable, IItemDropper where TItem : Component
{
    [Header("What drops")]
    [Tooltip("Set = always drops exactly this. Empty = one is drawn from the Random Pool below.")]
    [SerializeField] private TItem specificItem;

    [Tooltip("Used while Specific Item is empty. Fill this ONCE on the prefab: every egg " +
             "placed in the level inherits the list, and a single egg can still be given a " +
             "Specific Item without touching the others.")]
    [SerializeField] private TItem[] randomPool;

    [Header("Where it appears")]
    [Tooltip("Offset from this object where the item is created, before it is thrown.")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.1f);

    // Everything this dropper has put into the world, so a restart can take it back out.
    // A List and not a single field because nothing here promises to drop only once - an
    // enemy that respawns may be beaten again. WHO may drop how often is the caller's rule,
    // not ours.
    private readonly List<TItem> dropped = new List<TItem>();

    /// <summary>
    /// Creates one item and says where it is, or null if there was nothing to create. The
    /// triggers - the egg and a beaten enemy - only need to know WHERE the item is, so they
    /// get its Transform and never learn what kind of dropper this is (Dependency Inversion).
    ///
    /// The item is parented to OUR parent, not to us. That matters: it must keep lying
    /// where it fell even after the egg changes state, and it must still be switched off
    /// together with the level it belongs to.
    /// </summary>
    public Transform DropItem()
    {
        TItem prefab = ChooseItem();

        if (prefab == null)
        {
            Debug.LogError("[Drop] " + name + " has nothing to drop - set a Specific Item, " +
                           "or fill the Random Pool.", this);
            return null;
        }

        ForgetTakenItems();

        Vector3 position = transform.position + (Vector3)spawnOffset;
        TItem item = Instantiate(prefab, position, Quaternion.identity, transform.parent);

        dropped.Add(item);
        return item.transform;
    }

    /// <summary>
    /// Destroys what the player already picked up from earlier drops.
    ///
    /// A pickable that is taken only switches itself off - right for the ones placed in the
    /// level, which a new game switches back on, but a DROPPED one never comes back. Now
    /// that eggs and enemies respawn and drop again and again, those sleeping copies would
    /// pile up until the next restart; this keeps the list as short as what is still lying
    /// in the world.
    /// </summary>
    private void ForgetTakenItems()
    {
        for (int i = dropped.Count - 1; i >= 0; i--)
        {
            if (dropped[i] != null && dropped[i].gameObject.activeSelf)
                continue;

            if (dropped[i] != null)
                Destroy(dropped[i].gameObject);

            dropped.RemoveAt(i);
        }
    }

    /// <summary>
    /// A new game: whatever this dropper put into the world never happened.
    ///
    /// Destroy and not SetActive(false): these objects did not exist when the game started,
    /// so leaving them asleep in the hierarchy would pile up a new set on every restart.
    /// Unity's Destroy is deferred to the end of the frame, so an item that the restart
    /// sweep is about to reset on its own is still alive when its turn comes.
    /// </summary>
    public void ResetToStart()
    {
        for (int i = 0; i < dropped.Count; i++)
        {
            if (dropped[i] != null)
                Destroy(dropped[i].gameObject);
        }

        dropped.Clear();
    }

    /// <summary>
    /// The prefab to use, or null when this dropper was left unconfigured. A Specific Item,
    /// when one is set, always wins; otherwise one is drawn from the pool.
    /// </summary>
    private TItem ChooseItem()
    {
        if (specificItem != null)
            return specificItem;

        return PickRandom();
    }

    /// <summary>
    /// One entry out of the pool, ignoring empty slots - a half-filled list in the
    /// Inspector must not turn into "sometimes nothing comes out of the egg".
    ///
    /// Counted first and then walked, rather than copying the valid entries into a new
    /// list: no allocation, and the draw stays uniform.
    /// </summary>
    private TItem PickRandom()
    {
        if (randomPool == null)
            return null;

        int valid = 0;

        for (int i = 0; i < randomPool.Length; i++)
        {
            if (randomPool[i] != null)
                valid++;
        }

        if (valid == 0)
            return null;

        int pick = Random.Range(0, valid);

        for (int i = 0; i < randomPool.Length; i++)
        {
            if (randomPool[i] == null)
                continue;

            if (pick == 0)
                return randomPool[i];

            pick--;
        }

        return null;
    }
}
