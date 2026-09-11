using UnityEngine;

/// <summary>
/// An obstacle that costs power and shoves the player instead of killing him - the stone
/// from the assignment, which takes three segments off the bar.
///
/// It is the second PlayerContactEffect, next to KillPlayerOnTouch: detecting the player is
/// inherited, and the only thing written here is what the hit costs and which way it shoves.
///
/// It talks to IHurtable, so it never learns what a power bar is or how the recovery window
/// works - it states the price and lets the player decide whether the hit lands at all
/// (Dependency Inversion).
///
/// Nothing about being destructible is here either. The stone cannot be broken by any
/// weapon, so it carries no IDamageable and no health; the fairy will destroy it through
/// the same IKillable route everything else uses.
/// </summary>
public class HurtPlayerOnTouch : PlayerContactEffect
{
    [Tooltip("Power segments this costs. The assignment says 3 for the stone.")]
    [SerializeField] private int powerCost = 3;

    [Tooltip("Sideways speed of the shove, in units per second.")]
    [SerializeField] private float knockbackSpeed = 6f;

    [Tooltip("Upward part of the shove. A little lift reads better than a flat push.")]
    [SerializeField] private float knockbackLift = 4f;

    protected override void Affect(GameObject player)
    {
        IHurtable hurtable = player.GetComponent<IHurtable>();

        if (hurtable == null)
            return;

        hurtable.TryHurt(powerCost, BuildKnockback(player));
    }

    /// <summary>
    /// The shove carries the player ON in the direction he was already walking, the way the
    /// original game does it - he is tripped, not bounced back.
    ///
    /// The direction comes from IFacing, which the player already implements: he is the one
    /// who knows which way he was going, and asking him is cheaper and more reliable than
    /// guessing from which side of the stone he happens to stand (Dependency Inversion).
    /// </summary>
    private Vector2 BuildKnockback(GameObject player)
    {
        IFacing facing = player.GetComponent<IFacing>();

        // No IFacing: fall back to "away from this obstacle", which is at least never zero.
        float direction = facing != null
            ? (facing.FacingDirection >= 0f ? 1f : -1f)
            : (player.transform.position.x >= transform.position.x ? 1f : -1f);

        return new Vector2(direction * knockbackSpeed, knockbackLift);
    }
}
