using UnityEngine;

/// <summary>The star: gives Mario temporary invincibility.</summary>
public class StarController : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new StarPowerUp();
    }
}
