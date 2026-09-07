using Game.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// CONCRETE PRODUCT: the axe leaves the hand travelling forward and immediately starts to
/// fall - a plain downward parabola, y = -k * x^2, whose VERTEX is the moment of release.
///
/// It replaces exactly one inherited assumption - that a projectile flies in a straight
/// line at Stats.Speed - and inherits everything else: the Fire() template, the lifetime
/// timer, the shared IDamageable hit rule, the scenery rule, the pool handshake.
///
/// Both numbers that shape the curve live HERE, including the gravity. That is deliberate:
/// the flight path was previously split across the config asset, this component AND the
/// Rigidbody's own Gravity Scale, so tuning the arc meant editing three assets and
/// guessing which one was fighting the others. ApplyMovement is the hook the base class
/// provides for "how do I move", so the whole answer belongs in it.
/// </summary>
public sealed class ProjectileAxe : DirectionalProjectile
{
    [FormerlySerializedAs("speedY")]
    [Tooltip("Upward kick at the moment of release.\n" +
             "0 = the throw starts at the TOP of the arc and only ever falls - the plain " +
             "-x^2 parabola.\n" +
             "Above 0 = the axe rises first, so the vertex is out in front of the player.")]
    [SerializeField] private float upwardSpeed = 0f;

    [Tooltip("How sharply the axe drops - the k in y = -k * x^2. Higher falls faster.\n" +
             "This OVERWRITES the Rigidbody's Gravity Scale at launch, so the curve is " +
             "described in one place instead of two.")]
    [SerializeField] private float fallGravity = 1.5f;

    protected override string LogPrefix { get { return "[Axe]"; } }

    /// <summary>
    /// Set once, at launch, and physics draws the parabola from there - deliberately no
    /// Update. Constant forward speed plus constant downward acceleration IS y = -k * x^2;
    /// there is nothing to integrate by hand.
    /// </summary>
    protected override void ApplyMovement(Vector2 direction)
    {
        if (Body == null)
            return;

        // Written every launch, not once in Awake: a pooled axe is reused, and whoever
        // edits the prefab's Rigidbody later must not be able to change the flight path
        // by accident.
        Body.gravityScale = fallGravity;

        Body.linearVelocity = new Vector2(direction.x * Stats.Speed, upwardSpeed);
    }
}
