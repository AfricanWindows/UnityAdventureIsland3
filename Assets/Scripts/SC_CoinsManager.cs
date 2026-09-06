using System;
using UnityEngine;

public class SC_CoinsManager : MonoBehaviour, ICounter
{
    private int coins = 0;

    public int Value
    {
        get { return coins; }
    }

    public event Action<int> OnValueChanged;

    private void OnEnable()
    {
        SC_Coin.OnCoinCollision += OnCoinCollision;

        // Tell the UI where to find this counter (see CounterRegistry).
        CounterRegistry.Register(CounterId.Coins, this);
    }

    private void OnDisable()
    {
        SC_Coin.OnCoinCollision -= OnCoinCollision;
        CounterRegistry.Unregister(CounterId.Coins, this);
    }

    private void OnCoinCollision()
    {
        coins++;

        if (OnValueChanged != null)
            OnValueChanged(coins);
    }
}
