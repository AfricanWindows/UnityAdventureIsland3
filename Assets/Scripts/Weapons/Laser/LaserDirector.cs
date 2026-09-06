namespace Game.Projectiles
{
    /// <summary>
    /// The laser's recipe. It currently uses the standard order inherited from
    /// ProjectileDirector, and exists as the place to override Construct() the day the
    /// laser needs a step the other projectiles do not have - a charge-up, a beam length,
    /// a muzzle flash - without touching the generic director.
    /// </summary>
    public sealed class LaserDirector : ProjectileDirector<LaserProjectile>
    {
        public LaserDirector(IProjectileBuilder<LaserProjectile> builder) : base(builder) { }
    }
}
