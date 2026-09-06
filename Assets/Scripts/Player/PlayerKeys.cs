using System;
using UnityEngine;

/// <summary>
/// Remembers how many keys Mario picked up. Nothing else.
///
/// It is an ICounter, so the SAME UI_CounterView that shows coins and health can show
/// keys too - just pick CounterId.Keys on a label, no new UI class needed.
/// </summary>
public class PlayerKeys : MonoBehaviour, ICounter
{
    private int keys = 0;

    public bool HasKey
    {
        get { return keys > 0; }
    }

    public int Value
    {
        get { return keys; }
    }

    public event Action<int> OnValueChanged;

    private void OnEnable()
    {
        CounterRegistry.Register(CounterId.Keys, this);
        RaiseValueChanged();
    }

    private void OnDisable()
    {
        CounterRegistry.Unregister(CounterId.Keys, this);
    }

    public void AddKey(int amount)
    {
        if (amount <= 0)
            return;

        keys += amount;
        RaiseValueChanged();
    }

    private void RaiseValueChanged()
    {
        if (OnValueChanged != null)
            OnValueChanged(keys);
    }
}
