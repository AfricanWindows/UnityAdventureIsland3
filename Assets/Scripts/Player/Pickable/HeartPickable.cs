using UnityEngine;

/// <summary>
/// Exercise item 2: the heart Mario collects to get a health point back.
/// PlayerHealthModel refuses it when he already holds the maximum of 3.
/// </summary>
public class HeartPickable : BasePickable
{
    [SerializeField] private int healthAmount = 1;

    protected override IPowerUp CreatePowerUp()
    {
        return new HealthPowerUp(healthAmount);
    }
}
