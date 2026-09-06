using UnityEngine;

/// <summary>The axe: thrown forward AND upwards, so it arcs.</summary>
public class ProjectileAxe : BaseProjectile
{
    [SerializeField] private float speedX = 5f;
    [SerializeField] private float speedY = 5f;

    protected override void Launch(float direction)
    {
        Body.AddForce(new Vector2(direction * speedX, speedY));
    }
}
