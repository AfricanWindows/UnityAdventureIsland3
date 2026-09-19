using UnityEngine;

/// <summary>
/// "When this enemy is beaten, it may leave something behind" - the animal the assignment
/// says a destroyed enemy can drop.
///
/// It is the second WHEN over the same WHAT: the egg drops on touch, this drops on death,
/// and both hand the job to the very same PickableDropper next to them. That is the whole
/// reason the dropper was never written inside EggContainer (Open/Closed) - a third trigger,
/// say a smashed crate, is another small class like this one and no change anywhere else.
///
/// It talks to IRespawnable, not to BaseEnemy, so it never learns what an enemy is - only
/// that something announced it was defeated (Dependency Inversion).
///
/// Subscribed in Start and dropped only in OnDestroy - deliberately NOT the usual
/// OnEnable/OnDisable pair. The event we are waiting for is the one that DISABLES this
/// object, so an OnDisable unsubscribe would let go of it at the exact moment it fires.
/// Same reasoning as RespawnTimer, which listens to the same event.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PickableDropper))]
public class DropOnDefeat : MonoBehaviour
{
    [Tooltip("Chance to leave something behind. 1 = always, 0.25 = one beating in four.")]
    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    [Tooltip("What drops. Optional - taken from this object.")]
    [SerializeField] private PickableDropper dropper;

    [Tooltip("Optional arc. See the note below: a beaten enemy is switched off, so the item " +
             "lands immediately instead of flying. Leave it off unless you know why you want it.")]
    [SerializeField] private ItemToss toss;

    private IRespawnable target;

    private void Awake()
    {
        if (dropper == null)
            dropper = GetComponent<PickableDropper>();

        if (toss == null)
            toss = GetComponent<ItemToss>();

        target = GetComponent<IRespawnable>();

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

        BasePickable item = dropper.Drop();

        if (item == null)
            return;

        // Thrown to a random side, because unlike the egg there is no player standing here
        // to throw it away from. With no ItemToss it simply appears where the enemy fell,
        // which is what the original game does.
        if (toss != null)
            toss.Toss(item.transform, Random.value < 0.5f ? -1f : 1f);
    }
}
