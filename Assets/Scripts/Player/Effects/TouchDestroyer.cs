using UnityEngine;

/// <summary>
/// ROLE: While the fairy is active, destroys what the player touches (enemies, stones, fire).
/// PATTERNS: none - a plain component.
/// SOLID: D - destroys through IForceKillable.
///
/// "While this effect is running, everything I touch that can be destroyed, is" - the fairy
/// clearing enemies, stones and campfires out of the player's way.
///
/// It is the MIRROR of PlayerContactEffect. That class answers "something happens to the
/// player when he touches me" and lives on the hazard; this one answers "something happens
/// to what I touch" and lives on the player. Same four Unity messages, opposite direction,
/// which is why they are two classes and not one with a switch.
///
/// It destroys through IForceKillable, never through IDamageable, and that single choice is
/// what makes the assignment's rules fall out by themselves:
///   * an enemy dies, and its respawn timer brings it back as usual - BaseEnemy answers
///     ForceKill by dying, so no enemy needed a single component added to it;
///   * the campfire, which no weapon can scratch, and the stone, which the axe cannot, both
///     die, because they carry a Destructible and weapons do not go through this door;
///   * the ghost, which nothing else can touch, dies to the fairy for the same reason.
///
/// It asks a TimedPlayerEffect whether it is allowed to act, and not IInvincible: "the
/// player is currently immune" and "the player currently destroys what he touches" are two
/// different statements, and the death animation makes only the first one true.
///
/// The abyss is untouched by any of this - it is not something the player breaks, it is
/// something that breaks him, and it works through the same interface from the other side.
/// </summary>
[DisallowMultipleComponent]
public class TouchDestroyer : MonoBehaviour
{
    [Tooltip("What switches this on. Optional - taken from this object. Set it by hand if " +
             "the player ever carries more than one timed effect.")]
    [SerializeField] private TimedPlayerEffect effect;

    private void Awake()
    {
        if (effect == null)
            effect = GetComponent<TimedPlayerEffect>();

        if (effect == null)
            Debug.LogError("TouchDestroyer: no TimedPlayerEffect on " + gameObject.name +
                           " - it would never be allowed to destroy anything.", this);
    }

    // Both message pairs, because what the player runs into may be a solid obstacle (the
    // stone) or a trigger volume (the campfire), and this must not care which.
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null)
            TryDestroy(col.gameObject);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col != null)
            TryDestroy(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null)
            TryDestroy(col.gameObject);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col != null)
            TryDestroy(col.gameObject);
    }

    /// <summary>
    /// Stay and not only Enter: the player may pick the fairy up while he is ALREADY
    /// standing against a stone, and the touch that would have destroyed it happened a
    /// second before he was allowed to. Without this he would have to step back and walk
    /// into it again.
    /// </summary>
    private void TryDestroy(GameObject other)
    {
        // Cheapest test first: outside the effect this component costs one bool per contact.
        if (effect == null || !effect.IsActive)
            return;

        // InParent, because the collider that was hit is often a child of the object that
        // owns the behaviour - a hitbox on an enemy, a flame under a campfire.
        IForceKillable victim = other.GetComponentInParent<IForceKillable>();

        if (victim == null)
            return;

        victim.ForceKill();
    }
}
