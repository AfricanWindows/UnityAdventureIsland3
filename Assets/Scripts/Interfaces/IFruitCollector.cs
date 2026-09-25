/// <summary>
/// ROLE: "Count this fruit" - all a fruit needs from the fruit counter.
/// PATTERNS: none - a role interface.
/// SOLID: I - one method.
///
/// Something that counts the fruit the player eats.
///
/// One method, because that is all a fruit needs from the counter - the same reason
/// IPowerWallet and IExtraLife are one or two calls each (Interface Segregation).
/// </summary>
public interface IFruitCollector
{
    void Collect(int amount);
}
