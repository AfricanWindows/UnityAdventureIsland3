using UnityEngine;

/// <summary>
/// VIEW. Mirrors the object so it looks the way its IFacing says - the ghost, the frog and
/// both snakes turning towards what they act on.
///
/// It decides nothing. Which way to look is the enemy's own rule - the ghost looks at the
/// player, the snake looks where it hops or shoots - and each enemy answers it through
/// IFacing, the same interface the player's weapons already read. This only draws the
/// answer, so the four enemies share one view and none of them holds a SpriteRenderer or a
/// Transform flip of its own (Single Responsibility, Dependency Inversion).
///
/// It flips the SCALE, not SpriteRenderer.flipX, exactly like PlayerMovement does for the
/// player. flipX mirrors only the picture; the scale mirrors the children too, so a snake's
/// fire point in front of its mouth stays in front of its mouth when it turns round.
/// </summary>
[DisallowMultipleComponent]
public class FacingView : MonoBehaviour
{
    [Tooltip("Which way the drawing itself looks, before any flipping. Off = it looks LEFT, " +
             "which is how the ghost, the snakes and the frog are drawn.")]
    [SerializeField] private bool artFacesRight;

    private IFacing facing;

    // What is on screen right now, so the scale is written only when the answer CHANGES and
    // not every frame. 0 = nothing shown yet, so the first answer is always applied.
    private float shownDirection;

    private void Awake()
    {
        // InParent, so the view may sit on the root or on a child that holds only the sprite.
        facing = GetComponentInParent<IFacing>();

        if (facing == null)
            Debug.LogError("FacingView: nothing on " + gameObject.name + " implements " +
                           "IFacing - there is no direction to show.", this);
    }

    // LateUpdate: after the enemy has decided this frame, so the picture is never a frame late.
    private void LateUpdate()
    {
        if (facing == null)
            return;

        float direction = facing.FacingDirection;

        // No opinion, or nothing changed.
        if (direction == 0f || Mathf.Sign(direction) == shownDirection)
            return;

        shownDirection = Mathf.Sign(direction);

        float artDirection = artFacesRight ? 1f : -1f;

        // Abs, so a prefab that was scaled up in the editor keeps its size.
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * shownDirection * artDirection;
        transform.localScale = scale;
    }
}
