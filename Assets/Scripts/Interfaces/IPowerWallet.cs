/// <summary>
/// ROLE: The power bar as a resource: add segments, take segments.
/// PATTERNS: none - a role interface.
/// SOLID: I - the fruit and the stone see only these two calls.
///
/// The power bar seen as a resource: put segments in, take segments out.
///
/// What a stone and a fruit need from the power bar is exactly these two calls - not its
/// view, its drain clock or its config. Naming this and not PowerController keeps them
/// from depending on all of that (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IPowerWallet
{
    /// <summary>Adds up to the ceiling. Returns how many segments actually fitted.</summary>
    int AddPower(int amount);

    /// <summary>Takes away down to zero. Returns how many segments were actually taken.</summary>
    int RemovePower(int amount);
}
