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
/// EnemyRespawnTimer, and health is BaseEnemy - the same components every other enemy carries
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
public class PathEnemy : BaseEnemy, IActivatable
{
    // How much of the route the Scene view previews while the enemy is selected.
    private const float GizmoPreviewSeconds = 4f;
    private const int GizmoSegments = 40;

    private Rigidbody2D body;
    private MovementPath path;

    // Where the editor put it. The whole route is measured from here.
    private Vector2 origin;

    // Its OWN clock, not Time.time - so a revived or restarted enemy starts its route from the
    // beginning instead of appearing at a random point of it.
    private float travelTime;

    // Awake by default, so an enemy with no range simply moves. Only ActivateNearPlayer ever
    // turns this off - which matters for the bird: without it, it would fly off across the
    // level long before the player got there.
    private bool active = true;

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
    /// A whole-game restart. OnEnable does not run for an enemy that was never switched off, so
    /// the clock has to be put back here as well, or it would carry on from mid-route.
    /// </summary>
    public override void ResetToStart()
    {
        base.ResetToStart();
        travelTime = 0f;
    }

    /// <summary>The player came close. Carry on along the route.</summary>
    public void Activate()
    {
        active = true;
    }

    /// <summary>The player left. Freeze where it is; the clock stops with it.</summary>
    public void Deactivate()
    {
        active = false;
    }

    private void FixedUpdate()
    {
        if (!active || body == null || path == null)
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
