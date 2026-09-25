namespace Game.Weapons
{
    /// <summary>
    /// ROLE: The axe pool - its size is the axe's only limit.
    /// PATTERNS: Pooling; Generics - an empty closed type of ProjectilePoolManager.
    ///
    /// The axe's pool. Empty for the same engine reason as the others: Unity cannot show a
    /// generic MonoBehaviour in the Inspector, so a closed type must exist.
    ///
    /// Its Max Size IS the axe's only limit. The weapon has no ammunition - once picked up
    /// it throws forever - so what stops a held fire button is simply that every axe the
    /// pool owns is already in flight and none has landed yet. That cap refills by itself,
    /// which is why it is a pool size and not a counter.
    /// </summary>
    public class AxePoolManager : ProjectilePoolManager<ProjectileAxe>
    {
    }
}
