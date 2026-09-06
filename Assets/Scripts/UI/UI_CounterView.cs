using TMPro;
using UnityEngine;

/// <summary>
/// VIEW: shows any ICounter value in a TextMeshPro label.
/// Works for coins, health, axes... without changing this class (Open/Closed).
///
/// It does NOT hold a reference to the counter object, because Mario is spawned by the
/// Level Creator long after this label exists. It asks CounterRegistry for the id instead,
/// and rebinds automatically as soon as a matching counter appears.
/// </summary>
public class UI_CounterView : MonoBehaviour
{
    [Tooltip("Which value this label shows")]
    [SerializeField] private CounterId counterId = CounterId.Coins;

    [Tooltip("{0} is replaced by the counter value")]
    [SerializeField] private string format = "Coins: {0}";

    [Tooltip("Text shown while the counter does not exist yet")]
    [SerializeField] private int valueWhenMissing = 0;

    private TextMeshProUGUI label;
    private ICounter counter;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();

        if (label == null)
            Debug.LogError("UI_CounterView: no TextMeshProUGUI on " + gameObject.name, this);
    }

    private void OnEnable()
    {
        CounterRegistry.OnCounterRegistered += OnCounterRegistered;
        Bind(CounterRegistry.Get(counterId));
    }

    private void OnDisable()
    {
        CounterRegistry.OnCounterRegistered -= OnCounterRegistered;
        Bind(null);
    }

    private void OnCounterRegistered(CounterId id, ICounter newCounter)
    {
        if (id == counterId)
            Bind(newCounter);
    }

    private void Bind(ICounter newCounter)
    {
        if (counter == newCounter)
            return;

        if (counter != null)
            counter.OnValueChanged -= SetValue;

        counter = newCounter;

        if (counter != null)
        {
            counter.OnValueChanged += SetValue;
            SetValue(counter.Value);
        }
        else
        {
            SetValue(valueWhenMissing);
        }
    }

    private void SetValue(int value)
    {
        if (label != null)
            label.text = string.Format(format, value);
    }
}
