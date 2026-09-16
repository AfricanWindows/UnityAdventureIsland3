using UnityEngine;

/// <summary>
/// Bobs the object gently up and down, so a fruit lying on the ground looks like it is
/// waiting to be picked up instead of like a sprite someone forgot to animate.
///
/// It works on anything - a fruit, a weapon, the fairy - and it has no idea what it is
/// moving, so a new pickup gets it by dragging the component on (Single Responsibility).
///
/// EVERY COPY MOVES DIFFERENTLY. Twenty items driven by the same clock rise and fall as one
/// block, which looks like a bug rather than like life, so each one rolls its own starting
/// point in the wave and its own slightly different speed when it wakes up. Rolled in
/// OnEnable rather than on the prefab, because the prefab is ONE asset shared by every item
/// in the level - a number typed there would be the same number everywhere, which is the
/// problem itself.
///
/// The one clever bit: it notices when SOMEBODY ELSE moved the object and starts hovering
/// around the new place, without the item ever jumping. That is what lets it sit on a fruit
/// that is also being thrown out of an egg - it gives way while the throw is in charge, and
/// takes over from wherever the item landed. No connection between the two components, and
/// no execution order to get right (each stays usable on its own).
///
/// It bobs in LOCAL space, which is what makes it work on a child object as well: the fairy
/// sprite flying beside the player is parented to him, so hovering in world coordinates
/// would fight his own movement and the bob would flatten out the moment he walked. In local
/// space the item simply bobs beside whatever carries it. For an object with no moving
/// parent - every dropped item - local and world are the same thing.
/// </summary>
public class HoverEffect : MonoBehaviour
{
    [Tooltip("How fast it bobs.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("How far it rises above and sinks below its resting place, in units.")]
    [SerializeField] private float height = 0.12f;

    [Tooltip("How much the speed may differ from copy to copy. 0.2 = each item is up to " +
             "20% faster or slower than the number above. 0 = all exactly alike.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float speedVariation = 0.2f;

    // The point it hovers around. Not the position it was DESIGNED at - the last place it
    // was actually put, which for a dropped item is where it landed.
    private Vector3 origin;

    // The position this component wrote last frame. If the transform says something else
    // now, the move came from outside.
    private Vector3 lastApplied;

    // This copy's own place in the wave and its own speed. Two items that appear in the
    // same frame still never move together.
    private float phase;
    private float ownSpeed;

    private float time;

    private void OnEnable()
    {
        phase = Random.Range(0f, Mathf.PI * 2f);
        ownSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
        time = 0f;

        // The centre sits BELOW (or above) the current position by however far the wave is
        // from zero at this phase, so the very first frame draws the item exactly where it
        // was placed. Without this every item would twitch the moment it appeared.
        origin = transform.localPosition - new Vector3(0f, CurrentOffset(), 0f);
        lastApplied = transform.localPosition;
    }

    private void Update()
    {
        // Vector3's == is a fuzzy compare, which is exactly right here: it ignores floating
        // point noise and reacts only to a real move.
        if (transform.localPosition != lastApplied)
            origin = transform.localPosition - new Vector3(0f, CurrentOffset(), 0f);

        time += Time.deltaTime;

        Vector3 position = origin;
        position.y += CurrentOffset();

        transform.localPosition = position;
        lastApplied = position;
    }

    /// <summary>How far the wave is from its resting point right now.</summary>
    private float CurrentOffset()
    {
        return Mathf.Sin(time * ownSpeed + phase) * height;
    }
}
