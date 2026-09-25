using UnityEngine;

/// <summary>
/// ROLE: "A level has begun - reset yourself for it" (spawn point, power bar, fruit count).
/// PATTERNS: Observer-style - the level flow notifies every handler on the player.
/// SOLID: O - a new handler needs no edit in the flow.
///
/// "A level has just begun - put yourself in the state it should begin in."
///
/// Three things on the player need to know: where he now respawns (PlayerSpawn), that his
/// power bar starts full (PowerController), and that this level's fruit are counted from
/// zero (FruitCounterController). Before this interface existed, LevelFlowController
/// reached for the first two BY NAME - which meant the flow controller knew about
/// respawning and about power bars. The fruit counter, the third, would have been a third
/// GetComponent in there; instead it joined by implementing one method, and the flow
/// controller was not edited (Dependency Inversion, Open/Closed).
///
/// Now the flow announces once and whoever cares answers, exactly as PlayerDeath already
/// asks every IInvincible and PlayerMovement asks every IMovementLock.
///
/// The spawn position is passed to everyone even though only one listener uses it: it is
/// part of what "a level started" means, and an implementation that does not care simply
/// ignores the argument.
/// </summary>
public interface ILevelStartHandler
{
    void OnLevelStarted(Vector3 spawnPosition);
}
