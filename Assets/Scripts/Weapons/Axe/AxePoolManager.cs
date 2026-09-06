namespace Game.Weapons
{
    /// <summary>
    /// The axe's pool. Empty for the same engine reason as the others: Unity cannot show a
    /// generic MonoBehaviour in the Inspector, so a closed type must exist.
    ///
    /// Note that "how many axes Mario owns" is NOT decided here. The pool caps how many
    /// axes can be in the air at once; the ammo count is AmmoMagazine's job. Two different
    /// limits, two different classes.
    /// </summary>
    public class AxePoolManager : ProjectilePoolManager<ProjectileAxe>
    {
    }
}
