using UnityEngine;

/// <summary>
/// Exercise item 4: a floor that travels 2 tiles to the right and 2 tiles back.
///
/// It does NOT move the player. It only publishes how far it moved this physics step,
/// and whoever stands on it decides what to do with that number. Because of this the
/// platform never mentions Mario - a lift or a conveyor can reuse the same contract
/// without a single change here (SRP + Open/Closed).
///
/// Movement goes through Rigidbody2D.MovePosition in FixedUpdate, never through
/// transform.position in Update, so collisions and the passenger stay in sync.
/// </summary>
// Runs before the player, so the movement he reads this step is the CURRENT one
// and not the one from the previous step - that lag is what makes riding jitter.
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour, IRideablePlatform
{
    [Tooltip("Size of one tile in world units")]
    [SerializeField] private float tileSize = 1f;

    [Tooltip("How many tiles to travel before turning back")]
    [SerializeField] private float tilesDistance = 2f;

    [Tooltip("Travel speed in units per second")]
    [SerializeField] private float speed = 2f;

    private Rigidbody2D body;
    private Vector2 startPosition;
    private Vector2 delta;
    private float travelTime;

    public Vector2 Delta
    {
        get { return delta; }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = body.position;
        travelTime = 0f;
    }

    private void FixedUpdate()
    {
        Vector2 previous = body.position;

        // Its OWN clock, not Time.time: Time.time keeps running across a scene load,
        // so a restarted level would drop the platform in a random spot.
        travelTime += Time.fixedDeltaTime;

        // PingPong walks 0 -> distance -> 0 forever, which is exactly
        // "2 tiles right, then 2 tiles left".
        float distance = tileSize * tilesDistance;
        float offset = Mathf.PingPong(travelTime * speed, distance);

        Vector2 next = startPosition + Vector2.right * offset;

        body.MovePosition(next);
        delta = next - previous;
    }
}
