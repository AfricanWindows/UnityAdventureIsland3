using TMPro;
using UnityEngine;

/// <summary>
/// A VIEW that shows two numbers in one text label - "Lives: 3", "Fruits: 7/20".
///
/// It only draws. It holds no reference to a model, contains no rule, and never decides
/// when the value changes - a controller tells it what to show.
///
/// The lives label and the fruit label used to be two identical classes. What they share
/// is here once; each subclass only names its default format. They stay two classes (and
/// two view interfaces) because the DI container tells services apart by TYPE: the lives
/// controller must be handed the lives label, not the fruit label.
/// </summary>
public abstract class TextCounterView : MonoBehaviour
{
    [Tooltip("{0} = the current number, {1} = its limit (most lives, fruit per extra life). " +
             "Leave {1} out to show only the number.")]
    [SerializeField] private string format;

    private TextMeshProUGUI label;

    /// <summary>What the label says when nobody has typed a format in the Inspector.</summary>
    protected abstract string DefaultFormat { get; }

    // Unity calls Reset when the component is first added: the format field starts filled.
    private void Reset()
    {
        format = DefaultFormat;
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(format))
            format = DefaultFormat;

        label = GetComponent<TextMeshProUGUI>();

        if (label == null)
            Debug.LogError(GetType().Name + ": no TextMeshProUGUI on " + gameObject.name, this);
    }

    /// <summary>Fulfils IPlayerHealthView and IFruitCounterView for the subclasses.</summary>
    public void Render(int current, int limit)
    {
        if (label != null)
            label.text = string.Format(format, current, limit);
    }
}
