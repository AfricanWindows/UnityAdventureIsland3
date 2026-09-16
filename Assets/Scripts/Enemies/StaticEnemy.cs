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
/// Note it needs NO Rigidbody2D of its own. PathEnemy requires one because it MOVES its
/// body every physics step; a static enemy moves nothing. Give it a Static Rigidbody2D like
/// the shooting snake has and a solid collider, and KillPlayerOnTouch hears the contact
/// through the player's own body.
///
/// What goes on the prefab next to it: a Collider2D, KillPlayerOnTouch, and - only if it
/// should return after being beaten - an EnemyRespawnTimer. ActivateNearPlayer is pointless
/// here: it exists to put BEHAVIOUR to sleep, and there is none.
/// </summary>
public class StaticEnemy : BaseEnemy
{
}
