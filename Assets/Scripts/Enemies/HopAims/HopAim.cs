using UnityEngine;

/// <summary>
/// WHERE a hop goes: the sideways speed a hopping enemy takes off with. How high it goes and
/// how it falls belong to the JumpBehaviour; this only decides left, right and how far.
///
/// This is the Strategy half of HoppingEnemy. The snake and the frog share the whole hop
/// cycle - stand, jump, land, stand - and differ in exactly this answer: the snake always hops
/// the same way, the frog aims at the player. A new way of choosing where to hop is a new
/// subclass, with no edit to HoppingEnemy (Open/Closed).
///
/// A component found with GetComponent, the same way the enemy finds its JumpBehaviour.
/// </summary>
public abstract class HopAim : MonoBehaviour
{
    /// <summary>
    /// Asked once, at the moment of the push-off. The jump is handed in so an aim that has to
    /// land somewhere precise can ask how long the jump will stay in the air.
    /// </summary>
    public abstract float GetHorizontalSpeed(JumpBehaviour jump);
}
