using UnityEngine;

/// <summary>
/// Exercise item 6: walks right to left and back.
/// Turns around when it bumps into a wall. Optionally also turns after
/// a fixed distance, for platforms with no walls.
/// </summary>
public class PatrolEnemy : BaseEnemy
{
    [SerializeField] private float speed = 2f;

    [Tooltip("0 = turn only when hitting a wall. Above 0 = also turn after this distance.")]
    [SerializeField] private float patrolDistance = 0f;

    private Rigidbody2D rigid;
    private float startX;
    private float direction = -1f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        startX = transform.position.x;
        ApplyFacing();
    }

    private void FixedUpdate()
    {
        if (patrolDistance > 0f)
        {
            float x = transform.position.x;

            if (direction < 0f && x <= startX - patrolDistance)
                Turn(1f);
            else if (direction > 0f && x >= startX + patrolDistance)
                Turn(-1f);
        }

        if (rigid != null)
            rigid.linearVelocity = new Vector2(direction * speed, rigid.linearVelocity.y);
        else
            transform.Translate(new Vector3(direction * speed * Time.fixedDeltaTime, 0f, 0f));
    }

    protected override void OnCollisionEnter2D(Collision2D col)
    {
        base.OnCollisionEnter2D(col);
        TryTurnOnWall(col);
    }

    private void TryTurnOnWall(Collision2D col)
    {
        if (col == null)
            return;

        for (int i = 0; i < col.contactCount; i++)
        {
            Vector2 normal = col.GetContact(i).normal;

            // A mostly horizontal normal means we hit a wall, not the floor.
            if (Mathf.Abs(normal.x) > 0.5f)
            {
                Turn(normal.x > 0f ? 1f : -1f);
                return;
            }
        }
    }

    private void Turn(float newDirection)
    {
        if (direction == newDirection)
            return;

        direction = newDirection;
        startX = transform.position.x;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction >= 0f ? 1f : -1f);
        transform.localScale = scale;
    }
}
