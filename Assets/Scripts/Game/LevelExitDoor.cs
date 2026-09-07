using System;
using UnityEngine;

/// <summary>
/// The end of a level: the player touches it, the level is finished.
///
/// It only DETECTS. What happens next - switching to the next level, or showing the win
/// screen after the last one - belongs to LevelFlowController and LevelCompleteController
/// (Single Responsibility).
///
/// It used to demand a key first. That was a leftover from an earlier exercise and is not
/// in the final assignment, which says only "reach the end of the level, and the second
/// one starts at once". The key was also carried between levels, so the one found in
/// level one silently unlocked level two as well.
///
/// The event is STATIC so that any exit, in any level container, works without anybody
/// having to drag a reference in the Inspector.
/// </summary>
public class LevelExitDoor : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public static event Action OnLevelCompleted;

    // A trigger can report the same contact more than once - a player with a body collider
    // and a foot collider enters twice - and finishing the level twice would skip a level.
    private bool completed;

    // Cleared when the level is switched back on, so a replayed level can be finished again.
    private void OnEnable()
    {
        completed = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (completed || col == null || !col.gameObject.CompareTag(playerTag))
            return;

        completed = true;

        if (OnLevelCompleted != null)
            OnLevelCompleted();
    }
}
