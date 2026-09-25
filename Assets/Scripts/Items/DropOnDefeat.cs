using UnityEngine;

/// <summary>
/// "When this enemy is beaten, it may leave something behind" - the animal the assignment
/// says a destroyed enemy can drop.
///
/// It is the second WHEN over the same WHAT: the egg drops on touch, this drops on death,
/// and both hand the job to the very same IItemDropper next to them. That is the whole
/// reason the dropper was never written inside EggContainer (Open/Closed) - a third trigger,
/// say a smashed crate, is another small class like this one and no change anywhere else.
///
/// It talks to IDefeatable, not to BaseEnemy, and to IItemDropper, not to PickableDropper,
/// so it never learns what an enemy or a dropper is - only that something announced it was
/// defeated, and that something can drop an item (Dependency Inversion). Not IRespawnable
/// either: whether the enemy ever comes back is none of its business (Interface Segregation).
///
/// Subscribed in Start and dropped only in OnDestroy - deliberately NOT the usual
/// OnEnable/OnDisable pair. The event we are waiting for is the one that DISABLES this
/// object, so an OnDisable unsubscribe would let go of it at the exact moment it fires.
/// Same reasoning as RespawnTimer, which listens to the same event.
/// </summary>
[DisallowMultipleComponent]
public class DropOnDefeat : MonoBehaviour
{
    [Tooltip("Chance to leave something behind. 1 = always, 0.25 = one beating in four.")]
    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    // What drops. Found on this object; Unity cannot serialize an interface.
    private IItemDropper dropper;

    private IDefeatable target;

    private void Awake()
    {
        dropper = GetComponent<IItemDropper>();

        if (dropper == null)
            Debug.LogError("DropOnDefeat: no IItemDropper on " + gameObject.name + " - there is " +
                           "nothing to drop. Add a Pickable Dropper.", this);

        target = GetComponent<IDefeatable>();

        if (target == null)
            Debug.LogError("DropOnDefeat: nothing on " + gameObject.name + " can be defeated - " +
                           "this component will never fire. It belongs on an enemy.", this);
    }

    private void Start()
    {
        if (target != null)
        {
            target.Defeated -= OnDefeated;
            target.Defeated += OnDefeated;
        }
    }

    private void OnDestroy()
    {
        if (target != null)
            target.Defeated -= OnDefeated;
    }

    /// <summary>
    /// An enemy that comes back and is beaten again drops again - the dropper keeps every
    /// item it made and takes them all back on a restart, so nothing piles up.
    /// </summary>
    private void OnDefeated()
    {
        if (dropper == null)
            return;

        if (dropChance < 1f && Random.value > dropChance)
            return;

        // It appears where the enemy fell - no arc. A beaten enemy is switched off, and a
        // switched-off object cannot run the flight, so there is nothing to configure here.
        dropper.DropItem();
    }
}
