/// <summary>
/// A pickup that puts the player on an animal of type TMount.
///
/// Generic base so each animal pickup is one line: the blue, red and green pickups used to be
/// the same class three times with a different type name. The type is now the generic
/// parameter and the rest is written here once (Open/Closed, Generics, Factory Method) - the
/// twin of WeaponPickable, just as MountAnimalPowerUp is the twin of EquipWeaponPowerUp.
///
/// It stays abstract because Unity cannot put an open generic MonoBehaviour on a prefab -
/// each animal still needs its closed class.
/// </summary>
/// <typeparam name="TMount">The animal component this pickup puts the player on.</typeparam>
public abstract class MountPickable<TMount> : BasePickable
    where TMount : AnimalMount
{
    protected sealed override IPowerUp CreatePowerUp()
    {
        return new MountAnimalPowerUp<TMount>();
    }
}
