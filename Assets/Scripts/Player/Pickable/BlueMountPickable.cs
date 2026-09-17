/// <summary>
/// The pickup that puts the player on the blue animal - Sprite_PowerBlue.
///
/// Three lines, like AxePickable: being picked up is written once in BasePickable, mounting
/// is written once in MountAnimalPowerUp, and this only names which animal.
/// </summary>
public class BlueMountPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new MountAnimalPowerUp<BlueMount>();
    }
}
