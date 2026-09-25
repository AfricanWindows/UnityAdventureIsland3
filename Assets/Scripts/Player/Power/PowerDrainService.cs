using System;

/// <summary>
/// ROLE: The clock that takes one segment every N seconds.
/// PATTERNS: none - plain C# ticked from Update (deliberately NOT async - see below).
/// SOLID: D - it drains any IPowerModel.
///
/// The clock behind the power bar: one segment gone every few seconds, forever.
///
/// A plain C# class, not a MonoBehaviour: the controller owns it and calls Tick once per
/// frame from its Update. It does not know what a player is - it drains an IPowerModel
/// (Dependency Inversion), so the same class would run a boss timer or a bomb fuse.
///
/// WHY Update AND NOT ASYNC. This was an async loop once. But draining a bar is per-frame
/// bookkeeping, and per-frame work is exactly what Update is for - and Update is SAFE for
/// free: it stops when the game is paused (Time.deltaTime is 0 while Time.timeScale is 0),
/// it stops when the component is switched off, and it cannot outlive its object, so there
/// is no token to cancel and nothing that can leak after Play Mode. The project keeps async
/// for the one job that really needs it: RespawnTimer, which must keep counting while its
/// object is switched off.
/// </summary>
public class PowerDrainService : IPowerDrain
{
    private readonly IPowerModel _model;
    private readonly float _intervalSeconds;

    private float _elapsed;

    public PowerDrainService(IPowerModel model, float intervalSeconds)
    {
        if (model == null)
            throw new ArgumentNullException("model", "PowerDrainService needs a model to drain.");

        _model = model;

        // A zero or negative interval would drain the whole bar in one frame.
        _intervalSeconds = intervalSeconds > 0.05f ? intervalSeconds : 0.05f;
    }

    /// <summary>
    /// The interval starts again from zero. Without it the first segment after a respawn
    /// could vanish a fraction of a second later, because the old tick was half elapsed.
    /// </summary>
    public void Restart()
    {
        _elapsed = 0f;
    }

    /// <summary>Called once per frame with that frame's GAME time.</summary>
    public void Tick(float deltaTime)
    {
        _elapsed += deltaTime;

        // "while" and not "if": a long frame may cover more than one interval.
        while (_elapsed >= _intervalSeconds)
        {
            _elapsed -= _intervalSeconds;
            _model.Remove(1);
        }
    }
}
