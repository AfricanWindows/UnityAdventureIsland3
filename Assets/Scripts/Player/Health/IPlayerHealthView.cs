/// <summary>
/// ROLE: View contract of the lives.
/// PATTERNS: MVC (View contract); DI - registered by GameInstaller.
///
/// VIEW contract of the health feature (exercise item 2).
/// The controller pushes values in; the view never asks the model for anything.
/// </summary>
public interface IPlayerHealthView
{
    void Render(int current, int max);
}
