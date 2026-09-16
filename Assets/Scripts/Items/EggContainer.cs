using Game.Core;
using UnityEngine;

/// <summary>
/// The egg from the assignment: the player walks into it, it breaks open, and what was
/// inside - a weapon, an animal, the fairy - pops out and lands next to it.
///
/// This class is only the WHEN. What comes out is PickableDropper's business and how it
/// flies is ItemToss's, so an egg that should always hold a boomerang, an egg with a random
/// prize and a beaten enemy leaving an animal behind are three different arrangements of
/// the same two components and one script each (Single Responsibility, Open/Closed).
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
/// A broken egg is HIDDEN, not switched off. That is deliberate: this object is still
/// running the throw its contents are riding on, and it is where the broken-egg sprite will
/// go - swapping a sprite needs a renderer that still exists.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PickableDropper))]
public class EggContainer : PlayerContactEffect, IResettable
{
    [Tooltip("What is inside. Optional - taken from this object if left empty.")]
    [SerializeField] private PickableDropper dropper;

    [Tooltip("The little arc the contents fly along. Optional - leave the component off " +
             "the egg and the item simply appears in place.")]
    [SerializeField] private ItemToss toss;

    [Tooltip("The egg's own collider, switched off once it is broken. Optional - found here.")]
    [SerializeField] private Collider2D eggCollider;

    [Tooltip("The egg's sprite, hidden once it is broken. Optional - found here.")]
    [SerializeField] private SpriteRenderer eggRenderer;

    private bool opened;

    private void Awake()
    {
        if (dropper == null)
            dropper = GetComponent<PickableDropper>();

        if (toss == null)
            toss = GetComponent<ItemToss>();

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

        BasePickable item = dropper.Drop();

        if (item == null)
            return;

        opened = true;

        if (toss != null)
            toss.Toss(item.transform, DirectionAwayFrom(player));

        Hide();
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
