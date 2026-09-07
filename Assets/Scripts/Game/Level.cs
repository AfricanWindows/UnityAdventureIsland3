using UnityEngine;

/// <summary>
/// One level, living as a GameObject in the Hierarchy instead of as a scene.
///
/// It is deliberately DUMB: it holds the one thing only this level can answer - where the
/// player starts - and it switches itself on and off. It does not know that another level
/// exists, when it should be entered, or what happens when the player dies. That is
/// LevelFlowController's job (Single Responsibility).
///
/// Because there is no logic in here, level three is a copy of this component on a new
/// container and one more entry in the flow controller's list - no code (Open/Closed).
/// </summary>
public class Level : MonoBehaviour
{
    [Tooltip("Where the player appears when this level starts, and where he returns after " +
             "dying. Empty = this object's own position.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Shown in the console when the level is entered.")]
    [SerializeField] private string displayName = "";

    /// <summary>Where the player belongs in this level.</summary>
    public Vector3 SpawnPosition
    {
        get { return spawnPoint != null ? spawnPoint.position : transform.position; }
    }

    /// <summary>Name for the console. Falls back to the object's own name.</summary>
    public string DisplayName
    {
        get { return string.IsNullOrEmpty(displayName) ? name : displayName; }
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Draws the spawn point in the Scene view, so it can be placed by eye.</summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(SpawnPosition, 0.4f);
    }
}
