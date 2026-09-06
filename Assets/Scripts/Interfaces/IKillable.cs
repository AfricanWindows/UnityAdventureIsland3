/// <summary>
/// Something that can be killed by the game itself - starving, falling, drowning - as
/// opposed to being damaged. One method, so a caller that only needs to kill is not also
/// handed respawning, invincibility or the death event (Interface Segregation).
///
/// PowerController depends on THIS, not on PlayerDeath, so "the bar ran out" never has to
/// know how dying is implemented.
/// </summary>
public interface IKillable
{
    void Kill();
}
