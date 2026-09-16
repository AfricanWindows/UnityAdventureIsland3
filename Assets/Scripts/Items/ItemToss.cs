using System.Collections;
using UnityEngine;

/// <summary>
/// Throws an object along a short arc and leaves it where it lands - the little hop an
/// item makes when it comes out of an egg.
///
/// WHY NOT PHYSICS. Every pickup in this game (the axe, the fairy, the fruit) carries a
/// trigger collider and no Rigidbody2D, because a pickup is a thing you walk into, not a
/// thing the world pushes around. Throwing them with real physics would mean adding a body
/// and a solid collider to all of them, and then owning every question that follows: what
/// happens on a slope, at the edge of a platform, over a pit. A scripted arc costs three
/// numbers, lands the item exactly where the designer said, and touches none of the item
/// prefabs (they stay pure pickups).
///
/// The item lands at the SAME HEIGHT it started, so an egg standing on the ground always
/// drops its contents onto that same ground - no raycast, no ground layer, nothing to
/// configure per level.
///
/// It moves a Transform that belongs to somebody else and keeps no other link to it, so it
/// works for anything: an item out of an egg today, an animal out of a beaten enemy later.
/// </summary>
public class ItemToss : MonoBehaviour
{
    [Tooltip("How far the item travels sideways. The direction is given by the caller - " +
             "this is the distance, always positive.")]
    [SerializeField] private float distance = 1f;

    [Tooltip("How high above the start the arc peaks.")]
    [SerializeField] private float height = 1.2f;

    [Tooltip("How long the flight takes, in seconds.")]
    [SerializeField] private float flightTime = 0.45f;

    private Coroutine running;
    private Transform flying;
    private Vector3 landing;

    // The flying item's own colliders, switched off for the length of the flight.
    private Collider2D[] flyingColliders;

    /// <summary>
    /// Throws the item. Direction is +1 for right and -1 for left; anything else is read by
    /// its sign, so a caller may simply pass "player is on my left = 1".
    ///
    /// Asking for a direction instead of storing one is what lets the egg throw its contents
    /// AWAY from the player who broke it, without this class ever hearing of a player.
    /// </summary>
    public void Toss(Transform item, float direction)
    {
        if (item == null)
            return;

        // A second throw while the first is in the air finishes the first one properly
        // instead of abandoning an item halfway up.
        CancelFlight();

        Vector3 start = item.position;
        float sign = direction >= 0f ? 1f : -1f;

        flying = item;
        landing = start + new Vector3(sign * Mathf.Abs(distance), 0f, 0f);

        // An item in the air is not there to be touched yet, and switching its colliders
        // off for the flight is the whole of that rule.
        //
        // Without it the throw is invisible: the item is created INSIDE the player who
        // broke the egg, so its trigger fires on the very first physics step - before the
        // arc has moved it anywhere - and the prize is swallowed the instant it appears.
        // Throwing it further cannot help, because the pickup happens in the frame it is
        // born. Landing switches them back on, so a player still standing on the spot picks
        // it up the moment it touches the ground, which is what the original game does.
        flyingColliders = item.GetComponentsInChildren<Collider2D>(true);
        SetCollidersEnabled(false);

        // No flight possible: put the item straight down on its landing spot.
        //
        // isActiveAndEnabled is not paranoia. A beaten enemy is switched OFF before it
        // announces its defeat, and Unity refuses to start a coroutine on an inactive
        // object - StartCoroutine would throw instead of throwing the item. So a drop from
        // an enemy simply appears where it fell, exactly as in the original game, and only
        // an egg (which stays in the scene after it breaks) really flies.
        if (flightTime <= 0f || !isActiveAndEnabled)
        {
            FinishFlight();
            return;
        }

        running = StartCoroutine(Fly(start));
    }

    private IEnumerator Fly(Vector3 start)
    {
        float elapsed = 0f;

        while (elapsed < flightTime)
        {
            elapsed += Time.deltaTime;

            // The item can be destroyed mid-air by a restart. Nothing to finish then.
            if (flying == null)
            {
                running = null;
                FinishFlight();
                yield break;
            }

            float t = Mathf.Clamp01(elapsed / flightTime);

            // Straight line from start to landing, lifted by a parabola that is zero at
            // both ends and exactly "height" in the middle: 4t(1-t) peaks at 1 when t=0.5.
            Vector3 position = Vector3.Lerp(start, landing, t);
            position.y += height * 4f * t * (1f - t);

            flying.position = position;

            yield return null;
        }

        running = null;
        FinishFlight();
    }

    /// <summary>
    /// Unity kills the coroutines of a disabled object, and this one runs while the level
    /// may be switched off under it. Without this the item would hang in mid-air forever,
    /// at whatever point of the arc it had reached.
    /// </summary>
    private void OnDisable()
    {
        CancelFlight();
    }

    private void CancelFlight()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }

        FinishFlight();
    }

    /// <summary>
    /// Puts the item down on its landing spot and makes it touchable again. Safe to call
    /// twice, and safe when the item was destroyed under us mid-flight.
    /// </summary>
    private void FinishFlight()
    {
        if (flying != null)
            flying.position = landing;

        SetCollidersEnabled(true);

        flyingColliders = null;
        flying = null;
    }

    private void SetCollidersEnabled(bool value)
    {
        if (flyingColliders == null)
            return;

        for (int i = 0; i < flyingColliders.Length; i++)
        {
            if (flyingColliders[i] != null)
                flyingColliders[i].enabled = value;
        }
    }
}
