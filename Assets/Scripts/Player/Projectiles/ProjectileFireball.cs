using UnityEngine;

/// <summary>The fireball: flies straight ahead.</summary>
public class ProjectileFireball : BaseProjectile
{
    [SerializeField] private float speed = 5f;

    protected override void Launch(float direction)
    {
        Body.AddForce(new Vector2(direction * speed, 0f));
    }
}
