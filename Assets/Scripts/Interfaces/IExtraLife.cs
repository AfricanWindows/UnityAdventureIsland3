/// <summary>
/// ROLE: "Give one more life" - all the fruit counter needs from the lives.
/// PATTERNS: none - a role interface.
/// SOLID: I - one method.
///
/// Something that can be given one more life.
///
/// One method, because the fruit counter needs exactly one thing from the lives counter
/// (Interface Segregation), and it names this and not PlayerHealthController, so the fruit
/// rule never learns how lives are stored or shown (Dependency Inversion).
/// </summary>
public interface IExtraLife
{
    /// <summary>One life more, up to whatever ceiling the lives counter keeps.</summary>
    void GainLife();
}
