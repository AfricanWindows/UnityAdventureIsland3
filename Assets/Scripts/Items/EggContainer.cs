using System;
using Game.Core;
using UnityEngine;

/// <summary>
/// The egg from the assignment: the player walks into it, it breaks open, and what was
/// inside - a weapon, an animal, the fairy - pops out and lands next to it.
///
/// This class is only the WHEN. What comes out is the IItemDropper's business and how it
/// flies is the IItemToss's, so an egg that should always hold a boomerang, an egg with a random
/// prize and a beaten enemy leaving an animal behind are three different arrangements of
/// the same two components and one script each (Single Responsibility, Open/Closed).
/// Both are asked for as interfaces, so the egg never names PickableDropper or ItemToss
/// (Dependency Inversion).
///
/// Being touched by the player is not written here either - PlayerContactEffect already
/// owns that: which collider is the player, trigger or collision, and once per touch. This
/// class overrides the one step that is its own, exactly like the stone and the campfire
/// (Template Method).
///
/// Only touch breaks it, by choice: an egg that could also be opened with a thrown axe
/// would need health, an IDamageable and a decision about what a boomerang passing through
/// twice means. Nothing in the game needs that today. If it ever does, the trigger is one
/// more call to Open() and nothing below changes.
///
/// A broken egg is HIDDEN, not switched off. That is deliberate: the throw its contents are
/// riding on is a coroutine of the ItemToss on this very object, and Unity stops the
/// coroutines of a switched-off object - the arc would be cut short and the prize dropped
/// straight onto its landing spot.
///
/// It is IRespawnable, so a RespawnTimer dropped on the egg would make it whole again some
/// seconds after it was opened - the same component the enemies and the fruit use. Today no
/// egg carries one: an opened egg stays open until a new game (ResetToStart).
/// </summary>
[DisallowMultipleComponent]
public class EggContainer : PlayerContactEffect, IResettable, IRespawnable
{
    [Tooltip("The egg's own collider, switched off once it is broken. Optional - found here.")]
    [SerializeField] private Collider2D eggCollider;

    [Tooltip("The egg's sprite, hidden once it is broken. Optional - found here.")]
    [SerializeField] private SpriteRenderer eggRenderer;

    // What is inside - required. Found on this object; Unity cannot serialize an interface.
    private IItemDropper dropper;

    // The little arc the contents fly along - optional. Leave the component off the egg and
    // the item simply appears in place.
    private IItemToss toss;

    private bool opened;

    /// <summary>Raised the moment the egg breaks open. RespawnTimer listens.</summary>
    public event Action Defeated;

    /// <summary>True from the moment it is opened until it is whole again.</summary>
    public bool IsDefeated { get { return opened; } }

    private void Awake()
    {
        dropper = GetComponent<IItemDropper>();
        toss = GetComponent<IItemToss>();

        if (dropper == null)
            Debug.LogError("EggContainer: " + name + " has no IItemDropper - there is nothing " +
                           "inside it. Add a Pickable Dropper.", this);

        if (eggCollider == null)
            eggCollider = GetComponent<Collider2D>();

        if (eggRenderer == null)
            eggRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (eggCollider == null)
            Debug.LogError("EggContainer: " + name + " has no Collider2D - the player can " +
                           "never touch it. Add one and tick Is Trigger.", this);
    }

    /// <summary>The one step PlayerContactEffect leaves to us.</summary>
    protected override void Affect(GameObject player)
    {
        Open(player);
    }

    /// <summary>
    /// Breaks the egg once. If the dropper has nothing to give, the egg stays whole on
    /// purpose: an egg that opened and produced nothing looks like a game bug, while an egg
    /// that refuses to open next to a red error in the console looks like what it is.
    /// </summary>
    private void Open(GameObject player)
    {
        if (opened || dropper == null)
            return;

        Transform item = dropper.DropItem();

        if (item == null)
            return;

        opened = true;

        if (toss != null)
            toss.Toss(item, DirectionAwayFrom(player));

        Hide();

        if (Defeated != null)
            Defeated();
    }

    /// <summary>
    /// Which way the contents fly: away from whoever broke the egg, so the prize never
    /// lands behind the player's back.
    /// </summary>
    private float DirectionAwayFrom(GameObject player)
    {
        if (player == null)
            return 1f;

        return player.transform.position.x <= transform.position.x ? 1f : -1f;
    }

    private void Hide()
    {
        if (eggCollider != null)
            eggCollider.enabled = false;

        if (eggRenderer != null)
            eggRenderer.enabled = false;
    }

    /// <summary>
    /// A new game puts the egg back. Dying does NOT - the same rule the fruit already
    /// follows: what the player has already opened on this run stays opened when he walks
    /// the level again.
    ///
    /// What came OUT of the egg is not this class's to clean up; the dropper created it and
    /// removes it in its own ResetToStart.
    /// </summary>
    public void ResetToStart()
    {
        Close();
    }

    /// <summary>
    /// A RespawnTimer's countdown ran out: the egg is whole again and can be opened once
    /// more. Whatever it dropped last time stays where it landed.
    /// </summary>
    public void Revive()
    {
        Close();
    }

    private void Close()
    {
        opened = false;

        // The egg is never switched off, so the base class's OnEnable - the only other
        // place the "player is touching me" flag is cleared - does not run on a restart.
        ClearContact();

        if (eggCollider != null)
            eggCollider.enabled = true;

        if (eggRenderer != null)
            eggRenderer.enabled = true;
    }
}
