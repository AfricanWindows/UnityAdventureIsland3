/// <summary>
/// The dropper for things the player picks up: weapons, fruit, the fairy, and later the
/// animals and the fairy.
///
/// It is empty on purpose. Unity cannot add an open generic MonoBehaviour to a GameObject,
/// so a generic base needs one concrete subclass per closed type - exactly what
/// AxePoolManager is to ProjectilePoolManager. Naming BasePickable here is what makes the
/// Inspector refuse anything that is not a pickup.
/// </summary>
public class PickableDropper : ItemDropper<BasePickable>
{
}
