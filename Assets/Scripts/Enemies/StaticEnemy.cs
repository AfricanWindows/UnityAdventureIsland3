/// <summary>
/// An enemy that does nothing at all: it stands where it was placed, and touching it
/// kills Mario. A spike with a face.
///
/// The class is empty on purpose. Being damageable, dying, and killing the player on
/// contact are already written once in BaseEnemy - a static enemy is simply the base
/// behaviour with no behaviour added. Writing anything here would mean duplicating it.
///
/// It exists only because BaseEnemy is abstract, which it should stay: it is the shared
/// contract for enemies, not an enemy itself.
/// </summary>
public class StaticEnemy : BaseEnemy
{
}
