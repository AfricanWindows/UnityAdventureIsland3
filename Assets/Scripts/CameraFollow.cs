using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Keeps the camera on the player.
///
/// It does not search for him. It asks IPlayerProvider, which finds the object tagged
/// "Player" once and remembers it - so the tag rule is honoured, but the scene-wide search
/// happens at start-up instead of on every single frame in LateUpdate.
///
/// LateUpdate, not Update: the player moves in FixedUpdate/Update, and a camera that reads
/// his position before he has moved is a camera that shakes.
/// </summary>
public class CameraFollow : MonoBehaviour, IInjectable
{
    [Tooltip("Optional override. Normally left empty: the player arrives through injection.")]
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private IPlayerProvider players;

    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out players);
    }

    private void LateUpdate()
    {
        Transform follow = target != null ? target : ResolveTarget();

        if (follow == null)
            return;

        transform.position = follow.position + offset;
    }

    /// <summary>
    /// The provider caches, so this is a field read after the first frame. It is called
    /// again rather than cached HERE on purpose: the provider re-finds the player if he is
    /// ever replaced, and a copy kept in this class would go stale.
    /// </summary>
    private Transform ResolveTarget()
    {
        return players != null ? players.PlayerTransform : null;
    }
}
