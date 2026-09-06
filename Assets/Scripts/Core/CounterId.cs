/// <summary>
/// Every value a UI_CounterView can display.
/// Health is NOT here: it has its own MVC triad with a dedicated view
/// (PlayerHealthView), as the exercise requires.
/// The numbers are fixed so existing labels in the scene keep their setting.
/// </summary>
public enum CounterId
{
    Coins = 0,
    Axes = 2,
    Keys = 3
}
