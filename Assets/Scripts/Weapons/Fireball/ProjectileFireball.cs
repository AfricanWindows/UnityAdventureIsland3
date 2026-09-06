using Game.Projectiles;

/// <summary>
/// CONCRETE PRODUCT: the fireball flies straight ahead, the way Mario faces.
///
/// The class is empty on purpose. Facing, movement, damage, lifetime and the pool
/// handshake are all inherited - a fireball is simply a directional projectile with
/// nothing added. Its speed, damage and lifetime come from its ProjectileConfig asset,
/// so a stronger fireball is a new asset and no new code (Open/Closed).
///
/// It kept its name and its file GUID so the existing Fireball prefab still resolves;
/// only the base class underneath it changed.
/// </summary>
public sealed class ProjectileFireball : DirectionalProjectile
{
    protected override string LogPrefix { get { return "[Fireball]"; } }
}
