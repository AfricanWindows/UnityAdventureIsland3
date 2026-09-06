using System;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public static event Action OnPlayerDied;

    private Vector3 startPositon;

    private IInvincible[] invincibilitySources;

    private void OnEnable()
    {
        SC_Death.OnSpikeCollision += OnSpikeCollision;
    }

    private void OnDisable()
    {
        SC_Death.OnSpikeCollision -= OnSpikeCollision;
    }

    void Awake()
    {
        startPositon = transform.position;
        invincibilitySources = GetComponents<IInvincible>();
    }

    public void Respawn()
    {
        transform.position = startPositon;
    }

    /// <summary>Kills Mario: respawn + tell everyone (PlayerHealthController listens).</summary>
    public void Kill()
    {
        if (IsInvincible())
            return;

        Respawn();

        if (OnPlayerDied != null)
            OnPlayerDied();
    }

    /// <summary>
    /// True while ANY invincibility source is active - the star today,
    /// a shield or a hit-cooldown tomorrow, with no change needed here.
    /// </summary>
    private bool IsInvincible()
    {
        for (int i = 0; i < invincibilitySources.Length; i++)
        {
            if (invincibilitySources[i].IsInvincible)
                return true;
        }

        return false;
    }

    private void OnSpikeCollision()
    {
        Kill();
    }
}
