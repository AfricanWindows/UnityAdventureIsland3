using System;
using UnityEngine;

/// <summary>
/// Dying: back to the start of the current level, and a broadcast so the rest of the game
/// can react - the lives counter, the power bar and the weapons all listen.
///
/// It is the single implementation of IKillable, which is how enemies, their shots, spikes
/// and the two timers kill the player without any of them knowing what respawning is or
/// that a star can make him immune.
/// </summary>
public class PlayerDeath : MonoBehaviour, IKillable, ILevelStartHandler
{
    public static event Action OnPlayerDied;

    private Vector3 startPositon;

    private IInvincible[] invincibilitySources;
    private Rigidbody2D body;

    void Awake()
    {
        startPositon = transform.position;
        invincibilitySources = GetComponents<IInvincible>();
        body = GetComponent<Rigidbody2D>();
    }
    /// <summary>
    /// A level began: this is where the player now stands, and where dying will bring
    /// him back - level two sends him to level two, not to where the game started.
    /// </summary>
    public void OnLevelStarted(Vector3 spawnPosition)
    {
        startPositon = spawnPosition;
        Respawn();
    }

    /// <summary>
    /// Back to the start of the current level.
    ///
    /// The velocity is wiped too. Without it a player who died while falling arrives
    /// at the spawn point still falling at the speed that killed him, and drops
    /// straight through the floor on the frame he reappears.
    /// </summary>
    public void Respawn()
    {
        transform.position = startPositon;

        if (body != null)
            body.linearVelocity = Vector2.zero;
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

}
