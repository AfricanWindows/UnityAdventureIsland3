using Game.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// CONCRETE PRODUCT: the axe is thrown forward AND upwards, so it arcs down under gravity.
///
/// It replaces exactly one inherited assumption - that a projectile flies in a straight
/// line at Stats.Speed - and inherits everything else. The forward speed still comes from
/// the config asset; only the upward kick, which is what makes an axe an axe, is a field
/// on the prefab.
/// </summary>
public sealed class ProjectileAxe : DirectionalProjectile
{
    [FormerlySerializedAs("speedY")]
    [Tooltip("Upward launch speed. Gravity on the Rigidbody2D turns it into an arc. " +
             "This used to be a force, so it needs re-tuning once.")]
    [SerializeField] private float upwardSpeed = 6f;

    protected override string LogPrefix { get { return "[Axe]"; } }

    /// <summary>
    /// Set once, at launch, and physics carries it from there - deliberately no Update.
    /// </summary>
    protected override void ApplyMovement(Vector2 direction)
    {
        if (Body == null)
            return;

        Body.linearVelocity = new Vector2(direction.x * Stats.Speed, upwardSpeed);
    }
}
