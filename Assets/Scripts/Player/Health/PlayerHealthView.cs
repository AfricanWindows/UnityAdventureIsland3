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
    [Tooltip("{0} is the current health, {1} is the maximum")]
    [SerializeField] private string format = "Health: {0}/{1}";

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
