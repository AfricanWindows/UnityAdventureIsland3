/// <summary>
/// Which way its owner is looking: +1 right, -1 left.
///
/// Weapons depend on this abstraction instead of reading transform.localScale, so the
/// day the sprite flip changes (SpriteRenderer.flipX, an Animator, anything else),
/// aiming keeps working and no weapon has to be touched.
/// </summary>
public interface IFacing
{
    float FacingDirection { get; }
}
