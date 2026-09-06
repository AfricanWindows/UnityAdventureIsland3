using UnityEngine;

/// <summary>Exercise item 8: the key Mario needs before the door opens.</summary>
public class KeyPickable : BasePickable
{
    [SerializeField] private int keysAmount = 1;

    protected override IPowerUp CreatePowerUp()
    {
        return new KeyPowerUp(keysAmount);
    }
}
