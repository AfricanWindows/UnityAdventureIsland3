using System;
using Game.Core;
using UnityEngine;

/// <summary>
/// Base class for everything the player can pick up (fruit, weapons, the fairy...).
/// The "touch the player and disappear" logic is written here ONCE.
/// A child class only decides WHAT effect it gives, by creating an IPowerUp.
///
/// It is also IRespawnable, so a RespawnTimer next to it brings it back some seconds after
/// it was taken - the same component the enemies use. Nothing here counts time: a pickable
/// only says "I was taken" and "put me back" (Single Responsibility).
/// </summary>
public abstract class BasePickable : MonoBehaviour, IResettable, IRespawnable
{
    [SerializeField] private string playerTag = "Player";

    private bool collected = false;

    /// <summary>Raised the moment the player takes it. RespawnTimer listens.</summary>
    public event Action Defeated;

    /// <summary>True from the moment it is taken until it is put back.</summary>
    public bool IsDefeated { get { return collected; } }

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

        // Raised last, like BaseEnemy does: a listener that asks IsDefeated gets the truth.
        if (Defeated != null)
            Defeated();
    }

    /// <summary>
    /// Only the whole-game restart calls this. Dying does NOT, which is the rule:
    /// fruit already eaten stays eaten when the player returns to the start of the
    /// level. OnEnable clears the collected flag, so switching the object back on is
    /// the entire reset.
    /// </summary>
    public void ResetToStart()
    {
        gameObject.SetActive(true);
    }

    /// <summary>A RespawnTimer's countdown ran out: back on the spot it was taken from.</summary>
    public void Revive()
    {
        gameObject.SetActive(true);
    }

    /// <summary>Each pickable decides what it gives to the player.</summary>
    protected abstract IPowerUp CreatePowerUp();
}
