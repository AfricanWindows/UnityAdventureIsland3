using UnityEngine;

/// <summary>
/// ROLE: The SHAPE of a route: where to be after t seconds, measured from the start point.
/// PATTERNS: Strategy - the abstract route PathEnemy uses.
/// SOLID: O - a new route is a new subclass.
///
/// The SHAPE of a route: where the object should be, measured from where it was placed, after
/// a given number of seconds. That is all it answers. It never moves anything, never touches a
/// Rigidbody and knows nothing about enemies.
///
/// This is the Strategy half of PathEnemy. The spider and the bird share everything - a
/// kinematic body, a start point, their own clock, dying and coming back - and differ in one
/// formula. So the formula is the only thing that varies, and a new route is a new subclass
/// with no edit to PathEnemy or to the routes that already exist (Open/Closed).
///
/// A MonoBehaviour and not a plain class so that each route's numbers are set in the inspector
/// next to the enemy they belong to, and PathEnemy finds it with GetComponent - the same way
/// PlayerJump finds its JumpBehaviour.
///
/// The answer depends only on the time given, never on anything remembered between calls. That
/// is what makes it safe: the position cannot drift however long the level runs, and restarting
/// the route is nothing more than starting the clock at zero again.
/// </summary>
public abstract class MovementPath : MonoBehaviour
{
    /// <summary>Offset from the start point after this many seconds of travel.</summary>
    public abstract Vector2 GetOffset(float time);
}
