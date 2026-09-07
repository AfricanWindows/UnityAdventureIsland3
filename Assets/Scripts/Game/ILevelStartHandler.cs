using UnityEngine;

/// <summary>
/// "A level has just begun - put yourself in the state it should begin in."
///
/// Two things on the player need to know: where he now respawns, and that his timer starts
/// over. Before this interface existed, LevelFlowController reached for PlayerDeath and
/// PowerController BY NAME to tell them - which meant the flow controller knew about
/// respawning and about power bars, and a third thing that needed the same news would have
/// been a third GetComponent in there (Dependency Inversion, Open/Closed).
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
