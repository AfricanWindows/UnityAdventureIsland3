/// <summary>The pickup for the green animal - Sprite_PowerGreen.</summary>
public class GreenMountPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new MountAnimalPowerUp<GreenMount>();
    }
}
