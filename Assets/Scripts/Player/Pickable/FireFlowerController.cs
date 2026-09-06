using UnityEngine;

public class FireFlowerController : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new FireFlowerPowerUp();
    }
}
