using UnityEngine;

/// <summary>Pickable that gives Mario axes to throw (exercise item 2).</summary>
public class AxePickable : BasePickable
{
    [SerializeField] private int axesAmount = 1;

    protected override IPowerUp CreatePowerUp()
    {
        return new AxeAmmoPowerUp(axesAmount);
    }
}
