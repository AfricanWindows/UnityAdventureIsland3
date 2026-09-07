/// <summary>
/// VIEW contract of the fruit counter. The controller pushes values in; the view never
/// asks the model for anything.
///
/// It takes the threshold as well as the count because the label reads "Fruits: 7/20".
/// That second number lives in the model, so the view never repeats it - the mistake the
/// power bar made when its segment count was a separate serialised field.
/// </summary>
public interface IFruitCounterView
{
    void Render(int current, int threshold);
}
