using UnityEngine;

/// <summary>
/// A shot fired BY an enemy. It kills Mario and ignores enemies,
/// so enemies can never hurt each other.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speedX = 300f;
    [SerializeField] private float speedY = 0f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Launch(float direction)
    {
        transform.localScale = new Vector3(direction, 1f, 1f);

        if (rigid != null)
            rigid.AddForce(new Vector2(direction * speedX, speedY));

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col == null || !col.gameObject.CompareTag(playerTag))
            return;

        PlayerDeath playerDeath = col.gameObject.GetComponent<PlayerDeath>();
        if (playerDeath != null)
            playerDeath.Kill();

        Destroy(gameObject);
    }
}
