/// <summary>The pickup for the red animal - Sprite_PowerRed.</summary>
public class RedMountPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new MountAnimalPowerUp<RedMount>();
    }
}
