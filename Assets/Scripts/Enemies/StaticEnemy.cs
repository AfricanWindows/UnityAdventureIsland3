/// <summary>
/// An enemy that does nothing but exist: it hangs where the level put it, kills on contact,
/// can be beaten, and comes back. No route, no jump, no shot.
///
/// The body of this class is empty, and that is the point - EVERYTHING such an enemy needs
/// is already in BaseEnemy: health, TakeDamage, being switched off when beaten, Revive for
/// the respawn timer and ResetToStart for a new game. There was simply no way to put that
/// on a prefab, because BaseEnemy is abstract. This is the one line that gives it a
/// concrete, nameable form, exactly as PickableDropper does for ItemDropper.
///
/// It needs no Rigidbody2D of its own: PathEnemy requires one because it MOVES its body
/// every physics step, and this one moves nothing. A collider alone is enough - Unity
/// reports the contact between it and the player's own body, and KillPlayerOnTouch hears it.
/// A Rigidbody2D does no harm if one is there already; keep it Static or Kinematic, never
/// Dynamic, or the enemy would fall out of the level.
///
/// What goes on the prefab next to it: a Collider2D, KillPlayerOnTouch, and - only if it
/// should return after being beaten - a RespawnTimer. ActivateNearPlayer is pointless
/// here: it exists to put BEHAVIOUR to sleep, and there is none.
/// </summary>
public class StaticEnemy : BaseEnemy
{
}
