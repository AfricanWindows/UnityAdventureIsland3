using UnityEngine;

/// <summary>
/// Base class for everything Mario can pick up (fire flower, extra life, axes...).
/// The "touch the player and disappear" logic is written here ONCE.
/// A child class only decides WHAT effect it gives, by creating an IPowerUp.
/// </summary>
public abstract class BasePickable : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private bool collected = false;

    private void OnEnable()
    {
        collected = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (collected || col == null)
            return;

        if (!col.gameObject.CompareTag(playerTag))
            return;

        PlayerPowerUp playerPowerUp = col.gameObject.GetComponent<PlayerPowerUp>();
        if (playerPowerUp == null)
        {
            Debug.LogWarning("BasePickable: " + col.gameObject.name + " has no PlayerPowerUp component", this);
            return;
        }

        IPowerUp powerUp = CreatePowerUp();
        if (powerUp == null)
            return;

        collected = true;
        playerPowerUp.CollectPowerUp(powerUp);
        gameObject.SetActive(false);
    }

    /// <summary>Each pickable decides what it gives to the player.</summary>
    protected abstract IPowerUp CreatePowerUp();
}
