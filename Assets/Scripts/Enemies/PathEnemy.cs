using UnityEngine;

/// <summary>
/// An enemy that follows a fixed route through the air - the spider on its thread, the bird
/// flying across the screen.
///
/// It owns everything such an enemy has in common: a kinematic body, the point it was placed
/// at, its own clock, and pausing while asleep. The SHAPE of the route is not here - it belongs
/// to the MovementPath next to it (Strategy). A spider and a bird are the same class with a
/// different route component, and a new route never touches this file (Open/Closed).
///
/// Hurting the player on contact is KillPlayerOnTouch, coming back after being beaten is
/// RespawnTimer, and health is BaseEnemy - the same components every other enemy carries
/// (Single Responsibility).
///
/// The Rigidbody2D is meant to be KINEMATIC. Gravity would pull a dynamic enemy off its route,
/// and the player bumping into it would knock it aside. A kinematic body ignores both and goes
/// exactly where it is told, while still reporting contacts to KillPlayerOnTouch.
///
/// The position is COMPUTED every step from the start point, never accumulated, so it cannot
/// drift off its route however long the level runs.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PathEnemy : ActivatableEnemy
{
    // How much of the route the Scene view previews while the enemy is selected.
    private const float GizmoPreviewSeconds = 4f;
    private const int GizmoSegments = 40;

    private Rigidbody2D body;
    private MovementPath path;

    // The anchor the whole route is measured from. Where the editor put it to begin with,
    // and after being beaten, wherever it fell - see Revive.
    private Vector2 origin;

    // Its OWN clock, not Time.time - so a revived or restarted enemy starts its route from the
    // beginning instead of appearing at a random point of it.
    private float travelTime;

    protected override void OnAwake()
    {
        body = GetComponent<Rigidbody2D>();
        origin = body.position;

        // Asked for as the abstract base, so this class never names a concrete route.
        path = GetComponent<MovementPath>();

        if (path == null)
            Debug.LogError("PathEnemy: no MovementPath on " + gameObject.name +
                           " - add a Ping Pong Path or a Sine Path component.", this);
    }

    /// <summary>A beaten enemy that comes back starts its route from the beginning.</summary>
    private void OnEnable()
    {
        travelTime = 0f;
    }

    /// <summary>
    /// Comes back where it FELL, which is what the assignment asks for - and what this enemy
    /// could not do before.
    ///
    /// The route is measured from `origin`, and `origin` was captured once in OnAwake. So
    /// however carefully the base class left the body at the spot where it died, the very
    /// next FixedUpdate computed `origin + GetOffset(0)` and snapped it back to where the
    /// level had first placed it: Revive At Start Position was ticked for a spider and a bird
    /// whether the designer ticked it or not. Moving the ANCHOR, not just the body, is what
    /// actually keeps it where it died.
    ///
    /// transform.position and not body.position: the base class moves the enemy through the
    /// Transform, and a Rigidbody2D only picks that up at the next physics step - the
    /// Transform is correct at this instant either way.
    /// </summary>
    public override void Revive()
    {
        // The base class quietly ignores a Revive on a living enemy. Asked here as well, so
        // that such a call cannot drag a healthy enemy's route anchor to wherever it is now.
        if (!IsDefeated)
            return;

        base.Revive();

        origin = transform.position;
        travelTime = 0f;
    }

    /// <summary>
    /// A whole-game restart. Two things have to go back, not one: the clock, because OnEnable
    /// does not run for an enemy that was never switched off and it would otherwise carry on
    /// from mid-route - and the ANCHOR, because a Revive during the previous run has very
    /// likely moved it. Without the second line a restarted game would rebuild every route
    /// around the spot where that enemy last died.
    /// </summary>
    public override void ResetToStart()
    {
        base.ResetToStart();

        // The base class has just put the transform back where the level starts it.
        origin = transform.position;
        travelTime = 0f;
    }

    // Asleep (the player is far away) it freezes where it is, and its clock stops with it.
    private void FixedUpdate()
    {
        if (!IsActive || body == null || path == null)
            return;

        travelTime += Time.fixedDeltaTime;

        body.MovePosition(origin + path.GetOffset(travelTime));
    }

    /// <summary>
    /// Draws the route in the Scene view while the enemy is selected. It asks the route itself,
    /// so every route - existing or future - is drawn correctly with no drawing code of its own.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        MovementPath preview = path != null ? path : GetComponent<MovementPath>();
        if (preview == null)
            return;

        // Before Play the start point has not been captured yet - the transform IS the start.
        Vector3 start = Application.isPlaying ? (Vector3)origin : transform.position;

        Gizmos.color = Color.cyan;

        Vector3 previous = start + (Vector3)preview.GetOffset(0f);

        for (int i = 1; i <= GizmoSegments; i++)
        {
            float time = GizmoPreviewSeconds * i / GizmoSegments;
            Vector3 next = start + (Vector3)preview.GetOffset(time);

            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
