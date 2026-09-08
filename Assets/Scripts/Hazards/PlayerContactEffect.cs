using UnityEngine;

/// <summary>
/// TEMPLATE METHOD. "Something happens to the player when he touches me."
///
/// Detecting the player is the same three lines every time - is it a collision or a
/// trigger, is it really him, is he still there - and that skeleton is written HERE, once.
/// A subclass fills in the one step that differs: WHAT happens.
///
/// This used to live inside BaseEnemy, which meant an enemy was two things at once: a
/// thing that can be hurt, and a thing that hurts. The campfire and the stone from the
/// assignment are the second without being the first - a campfire kills on contact but is
/// not an enemy and cannot be damaged - so keeping the rule inside BaseEnemy would have
/// forced them to copy it (Single Responsibility, Open/Closed).
///
/// Both message methods are handled, because the same effect must work on a solid obstacle
/// (collision) and on a trigger volume (fire, water) without the author having to know
/// which one Unity will call.
/// </summary>
public abstract class PlayerContactEffect : MonoBehaviour
{
    [Tooltip("Only objects with this tag are affected.")]
    [SerializeField] private string playerTag = "Player";

    // Virtual, not private: PatrolEnemy also reads collisions, to turn around at walls.
    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null)
            TryAffect(col.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null)
            TryAffect(col.gameObject);
    }

    private void TryAffect(GameObject other)
    {
        // CompareTag, not other.tag == "...": comparing the property allocates a managed
        // string on every single contact.
        if (!other.CompareTag(playerTag))
            return;

        Affect(other);
    }

    /// <summary>The one step each hazard defines for itself.</summary>
    protected abstract void Affect(GameObject player);
}
