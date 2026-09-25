/// <summary>
/// ROLE: View of the fruit counter ("Fruits: 7/20").
/// PATTERNS: MVC (View) - a TextCounterView with its own type for DI.
///
/// VIEW of the fruit counter: one label reading "Fruits: 7/20". Everything it does is
/// TextCounterView's; this class only gives it the fruit label's default text and its own
/// type for the DI container.
/// </summary>
public class FruitCounterView : TextCounterView, IFruitCounterView
{
    protected override string DefaultFormat { get { return "Fruits: {0}/{1}"; } }
}
