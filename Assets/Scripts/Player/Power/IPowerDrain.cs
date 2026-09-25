/// <summary>
/// ROLE: Contract of the drain clock: Tick and Restart.
/// PATTERNS: none - a role interface.
/// SOLID: D - PowerController never names PowerDrainService.
///
/// The clock that eats the power bar: tick it every frame, restart it when the run restarts.
///
/// PowerController depends on THIS rather than on PowerDrainService, so the rule "power leaks
/// away as time passes" is separable from the one implementation of it that exists today
/// (Dependency Inversion). A drain that speeds up on the second level, or a silent one used
/// while testing, is a new class and not one edit in the controller.
///
/// Two members and no more - the two the controller actually calls (Interface Segregation).
/// </summary>
public interface IPowerDrain
{
    /// <summary>Called once per frame with that frame's GAME time.</summary>
    void Tick(float deltaTime);

    /// <summary>The interval starts again from zero - a new level, or a respawn.</summary>
    void Restart();
}
