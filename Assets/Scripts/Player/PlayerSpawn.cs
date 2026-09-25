using UnityEngine;

/// <summary>
/// ROLE: Remembers where the current level starts and moves the player there.
/// PATTERNS: none - a plain component.
/// SOLID: S - split out of PlayerDeath.
///
/// Where the player (re)appears: the start of the current level.
///
/// It remembers the spawn point and moves the player there - two moments use it. A level
/// begins (ILevelStartHandler, called by LevelFlowController): the point changes and he is
/// placed on it. He dies (IPlayerSpawn, called by PlayerDeath once the animation is over): he
/// is put back on the SAME point - level two sends him to level two, not to where the game
/// started.
///
/// This used to live inside PlayerDeath, which made dying also the owner of the level's
/// start position. Placing the player at the start of a level is not a death, so it is a
/// class of its own now (Single Responsibility).
/// </summary>
[DisallowMultipleComponent]
public class PlayerSpawn : MonoBehaviour, IPlayerSpawn, ILevelStartHandler
{
    private Vector3 spawnPosition;
    private Rigidbody2D body;

    private void Awake()
    {
        // Until the first level says otherwise, the start is where the editor put him.
        spawnPosition = transform.position;
        body = GetComponent<Rigidbody2D>();
    }

    /// <summary>A level began: this is the new start, and he stands on it now.</summary>
    public void OnLevelStarted(Vector3 levelSpawnPosition)
    {
        spawnPosition = levelSpawnPosition;
        ReturnToSpawn();
    }

    /// <summary>
    /// Back to the start of the current level.
    ///
    /// The velocity is wiped too. Without it a player who died while falling arrives at the
    /// spawn point still falling at the speed that killed him, and drops straight through
    /// the floor on the frame he reappears.
    /// </summary>
    public void ReturnToSpawn()
    {
        transform.position = spawnPosition;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}
