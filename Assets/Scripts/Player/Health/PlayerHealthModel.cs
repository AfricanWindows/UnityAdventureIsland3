using System;

/// <summary>
/// MODEL of the health feature (exercise item 2).
///
/// It owns the DATA and the RULES only:
///   - health can never go above the maximum (3 hearts)
///   - health can never go below zero
///   - it reports what happened through events
///
/// There is no UnityEngine reference in this file on purpose: the model knows nothing
/// about GameObjects, colliders or UI. That is exactly what makes it a Model.
/// </summary>
public class PlayerHealthModel : IPlayerHealthModel
{
    private readonly int maxHealth;
    private int currentHealth;

    public PlayerHealthModel(int maxHealth, int startHealth)
    {
        this.maxHealth = maxHealth < 1 ? 1 : maxHealth;
        currentHealth = Clamp(startHealth);
    }

    public int Current
    {
        get { return currentHealth; }
    }

    public int Max
    {
        get { return maxHealth; }
    }

    public bool IsFull
    {
        get { return currentHealth >= maxHealth; }
    }

    public event Action Changed;

    public event Action Empty;

    /// <summary>
    /// Collecting a heart. Returns false when Mario is already at 3 hearts,
    /// so the caller can tell that the heart had no effect.
    /// </summary>
    public bool Add(int amount)
    {
        if (amount <= 0 || IsFull)
            return false;

        SetHealth(currentHealth + amount);
        return true;
    }

    /// <summary>Landing on spikes.</summary>
    public void Remove(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        SetHealth(currentHealth - amount);

        if (currentHealth <= 0 && Empty != null)
            Empty();
    }

    private void SetHealth(int value)
    {
        int clamped = Clamp(value);
        if (clamped == currentHealth)
            return;

        currentHealth = clamped;

        if (Changed != null)
            Changed();
    }

    private int Clamp(int value)
    {
        if (value < 0)
            return 0;

        if (value > maxHealth)
            return maxHealth;

        return value;
    }
}
