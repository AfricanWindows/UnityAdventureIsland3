namespace Game.Projectiles
{
    /// <summary>
    /// The boomerang's recipe. Like LaserDirector, it uses the standard step order
    /// inherited from ProjectileDirector and exists as the place to override Construct()
    /// the day the boomerang needs a step the other projectiles do not - without touching
    /// the generic director.
    /// </summary>
    public sealed class BoomerangDirector : ProjectileDirector<BoomerangProjectile>
    {
        public BoomerangDirector(IProjectileBuilder<BoomerangProjectile> builder) : base(builder) { }
    }
}
