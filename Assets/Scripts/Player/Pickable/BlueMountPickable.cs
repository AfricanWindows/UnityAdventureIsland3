/// <summary>
/// ROLE: The pickup that puts the player on the blue animal.
/// PATTERNS: Factory Method - inherited from MountPickable; Generics - one line.
///
/// The pickup that puts the player on the blue animal - Sprite_PowerBlue.
///
/// One line, like AxePickable: being picked up is written once in BasePickable, mounting is
/// written once in MountPickable and MountAnimalPowerUp, and this only names which animal.
/// </summary>
public class BlueMountPickable : MountPickable<BlueMount>
{
}
