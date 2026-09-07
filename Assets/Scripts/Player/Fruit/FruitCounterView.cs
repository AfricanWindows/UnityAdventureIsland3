using TMPro;
using UnityEngine;

/// <summary>
/// VIEW of the fruit counter: one label reading "Fruits: 7/20".
///
/// It only draws. It holds no reference to the model, contains no rule about laps, and
/// never decides when the value changes - it is told what to show. Built exactly like
/// PlayerHealthView, on purpose: one way of doing a text readout in this project, not two.
/// </summary>
public class FruitCounterView : MonoBehaviour, IFruitCounterView
{
    [Tooltip("{0} is the fruit eaten so far, {1} is how many make a lap")]
    [SerializeField] private string format = "Fruits: {0}/{1}";

    private TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();

        if (label == null)
            Debug.LogError("FruitCounterView: no TextMeshProUGUI on " + gameObject.name, this);
    }

    public void Render(int current, int threshold)
    {
        if (label != null)
            label.text = string.Format(format, current, threshold);
    }
}
