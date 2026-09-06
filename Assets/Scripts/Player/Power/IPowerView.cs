/// <summary>
/// VIEW contract of the power bar. The controller pushes values in; the view never asks
/// the model for anything and holds no reference to it.
///
/// Two ints is the whole contract (Interface Segregation): a bar made of sprites, a
/// number in a label, or a silent test double all satisfy it.
/// </summary>
public interface IPowerView
{
    void Render(int current, int max);
}
