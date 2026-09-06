using UnityEngine;

/// <summary>
/// Bonus: keeps one enemy alive at this spot. When its enemy is destroyed,
/// a new one appears here after respawnDelay seconds.
///
/// It never asks WHAT it is spawning - any enemy prefab fits the same field,
/// so adding a new enemy type needs no change in this class.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Seconds to wait after the enemy dies before a new one appears")]
    [SerializeField] private float respawnDelay = 5f;

    [Tooltip("Create the first enemy when the level starts")]
    [SerializeField] private bool spawnOnStart = true;

    private GameObject currentEnemy;
    private float timer = 0f;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab is not assigned", this);
            return;
        }

        if (spawnOnStart)
            Spawn();
    }

    private void Update()
    {
        if (enemyPrefab == null)
            return;

        // Unity returns null for a destroyed object, so this is the death check.
        if (currentEnemy != null)
            return;

        timer += Time.deltaTime;

        if (timer < respawnDelay)
            return;

        Spawn();
    }

    private void Spawn()
    {
        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        timer = 0f;
    }

    // Makes the empty spawner visible while building the level.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
