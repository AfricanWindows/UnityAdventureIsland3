/// <summary>
/// ROLE: View of the lives ("Lives: 3").
/// PATTERNS: MVC (View) - a TextCounterView with its own type for DI.
///
/// VIEW of the lives counter. Everything it does is TextCounterView's; this class only
/// gives it the lives label's default text and its own type for the DI container.
/// </summary>
public class PlayerHealthView : TextCounterView, IPlayerHealthView
{
    protected override string DefaultFormat { get { return "Lives: {0}"; } }
}
