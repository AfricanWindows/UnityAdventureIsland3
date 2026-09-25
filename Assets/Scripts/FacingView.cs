using UnityEngine;

/// <summary>
/// ROLE: View - mirrors the sprite to the side its owner faces (the player, the ghost).
/// PATTERNS: MVC-style View - draws the IFacing answer and decides nothing.
/// SOLID: S - one flip mechanism for everyone; D - reads IFacing, not a concrete class.
///
/// VIEW. Mirrors the object so it looks the way its IFacing says. Two things in the game turn:
/// the player, who walks both ways, and the ghost, which chases him both ways. The snakes and
/// the frog always face left, as in the original game, so they carry no IFacing and no
/// FacingView.
///
/// It decides nothing. Which way to look is the owner's own rule, answered through IFacing -
/// the same interface the weapons already read to know which way to throw. This only draws the
/// answer, so neither PlayerMovement nor an enemy holds a Transform flip of its own (Single
/// Responsibility, Dependency Inversion), and there is ONE flipping mechanism in the project
/// instead of one per character.
///
/// It flips the SCALE, not SpriteRenderer.flipX. flipX mirrors only the picture; the scale
/// mirrors the children too, so a child in front of the face - a fire point, say - stays in
/// front when it turns round.
/// </summary>
[DisallowMultipleComponent]
public class FacingView : MonoBehaviour
{
    [Tooltip("Which way the drawing itself looks, before any flipping. Off = it looks LEFT, " +
             "which is how the enemies are drawn. ON for the player, who is drawn facing RIGHT.")]
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
