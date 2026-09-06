using System;
using UnityEngine;

/// <summary>
/// Exercise item 8: the door that ends the level - but only if Mario
/// already collected the key. It only DETECTS; showing the win screen
/// is LevelCompleteController's job.
///
/// The event is STATIC so that any door placed by the Level Creator works,
/// without anybody having to drag a reference in the Inspector.
/// </summary>
public class LevelExitDoor : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public static event Action OnLevelCompleted;

    private bool completed = false;

    private void OnEnable()
    {
        completed = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (completed || col == null || !col.gameObject.CompareTag(playerTag))
            return;

        PlayerKeys playerKeys = col.gameObject.GetComponent<PlayerKeys>();

        if (playerKeys == null || !playerKeys.HasKey)
        {
            Debug.Log("<color=yellow>Door is locked - find the key first</color>");
            return;
        }

        completed = true;

        if (OnLevelCompleted != null)
            OnLevelCompleted();
    }
}
