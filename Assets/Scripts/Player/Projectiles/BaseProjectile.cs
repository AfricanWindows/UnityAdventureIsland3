using UnityEngine;

/// <summary>
/// Everything every projectile shares: it faces the way it flies, it damages what it
/// hits, and it disappears after a while. That is written HERE, once.
///
/// HOW it flies is the only thing a child class decides, through Launch().
/// A new projectile means one small class - nothing in here has to change (OCP).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    [Tooltip("Seconds before the projectile removes itself")]
    [SerializeField] private float lifetime = 3f;

    /// <summary>The body a child class pushes in Launch().</summary>
    protected Rigidbody2D Body { get; private set; }

    private void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    /// <summary>Fired by a weapon. direction is +1 right, -1 left.</summary>
    public void Attack(float direction)
    {
        if (Body == null)
            return;

        transform.localScale = new Vector3(direction, 1, 1);
        Launch(direction);
        Destroy(gameObject, lifetime);
    }

    /// <summary>Each projectile decides how it moves.</summary>
    protected abstract void Launch(float direction);

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col == null)
            return;

        IDamageable target = col.GetComponent<IDamageable>();
        if (target == null)
            return;

        target.TakeDamage(damage);
        Destroy(gameObject);
    }
}
