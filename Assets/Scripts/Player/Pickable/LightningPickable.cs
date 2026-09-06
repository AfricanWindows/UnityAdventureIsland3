using UnityEngine;

/// <summary>Exercise item 1: the lightning bolt Mario collects for a speed boost.</summary>
public class LightningPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new SpeedPowerUp();
    }
}
