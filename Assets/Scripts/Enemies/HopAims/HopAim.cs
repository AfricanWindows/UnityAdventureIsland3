using UnityEngine;

/// <summary>
/// WHERE a hop goes: the sideways speed a hopping enemy takes off with. How high it goes and
/// how it falls belong to the JumpBehaviour; this only decides how far to the left.
///
/// This is the Strategy half of HoppingEnemy. The snake and the frog share the whole hop
/// cycle - stand, jump, land, stand - and differ in exactly this answer: the snake always hops
/// the same distance, the frog aims at the player. A new way of choosing where to hop is a new
/// subclass, with no edit to HoppingEnemy (Open/Closed).
///
/// LEFT ONLY, as in the original game: no enemy there ever hops to the right. So every answer
/// is zero or negative, and the enemy never has to turn round.
///
/// A component found with GetComponent, the same way the enemy finds its JumpBehaviour.
/// </summary>
public abstract class HopAim : MonoBehaviour
{
    /// <summary>
    /// Asked once, at the moment of the push-off. Zero or negative: 0 = straight up on the
    /// spot, below 0 = to the left. The jump is handed in so an aim that has to land somewhere
    /// precise can ask how long the jump will stay in the air.
    /// </summary>
    public abstract float GetHorizontalSpeed(JumpBehaviour jump);
}
