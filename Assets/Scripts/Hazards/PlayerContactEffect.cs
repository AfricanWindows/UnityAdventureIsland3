using UnityEngine;

/// <summary>
/// TEMPLATE METHOD. "Something happens to the player when he touches me, once per touch."
///
/// Detecting the player is the same few lines every time - is it a collision or a trigger,
/// is it really him, is this the same touch as a moment ago - and that skeleton is written
/// HERE, once. A subclass fills in the one step that differs: WHAT happens.
///
/// ONCE PER TOUCH is the important half. A hazard the player is standing in keeps
/// reporting contact, so without this a stone would take three power segments per physics
/// step and empty the bar in a blink. The flag is cleared when he leaves, which means
/// walking back in hits again immediately - no timer, no cooldown to tune, and two stones
/// side by side cost two hits, exactly as they should.
///
/// This used to live inside BaseEnemy, which made an enemy two things at once: a thing
/// that can be hurt, and a thing that hurts. The campfire and the stone are the second
/// without being the first (Single Responsibility, Open/Closed).
///
/// Both message pairs are handled, because the same effect must work on a solid obstacle
/// (collision) and on a trigger volume (fire, water) without the author choosing.
/// </summary>
public abstract class PlayerContactEffect : MonoBehaviour
{
    [Tooltip("Only objects with this tag are affected.")]
    [SerializeField] private string playerTag = "Player";

    // One touch = one effect. Cleared on exit, and on enable so a hazard that was switched
    // off mid-contact does not stay armed against a player who is long gone.
    private bool playerInside;

    protected virtual void OnEnable()
    {
        playerInside = false;
    }

    // Virtual, not private: PatrolEnemy also reads collisions, to turn around at walls.
    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null)
            TryAffect(col.gameObject);
    }

    protected virtual void OnCollisionExit2D(Collision2D col)
    {
        if (col != null)
            ClearIfPlayer(col.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null)
            TryAffect(col.gameObject);
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
            ClearIfPlayer(col.gameObject);
    }

    private void TryAffect(GameObject other)
    {
        // CompareTag, not other.tag == "...": comparing the property allocates a managed
        // string on every single contact.
        if (playerInside || !other.CompareTag(playerTag))
            return;

        playerInside = true;
        Affect(other);
    }

    private void ClearIfPlayer(GameObject other)
    {
        if (other.CompareTag(playerTag))
            playerInside = false;
    }

    /// <summary>The one step each hazard defines for itself.</summary>
    protected abstract void Affect(GameObject player);
}
