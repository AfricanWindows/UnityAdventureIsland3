using TMPro;
using UnityEngine;

/// <summary>
/// VIEW of the health feature (exercise item 2).
///
/// It only draws. It holds no reference to the model, contains no health rule, and
/// never decides when the value changes - it is told what to show.
/// </summary>
public class PlayerHealthView : MonoBehaviour, IPlayerHealthView
{
    [Tooltip("{0} is the lives left, {1} is the most the player can hold. Leave {1} out to " +
             "show only the lives.")]
    [SerializeField] private string format = "Lives: {0}";

    private TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();

        if (label == null)
            Debug.LogError("PlayerHealthView: no TextMeshProUGUI on " + gameObject.name, this);
    }

    public void Render(int current, int max)
    {
        if (label != null)
            label.text = string.Format(format, current, max);
    }
}
