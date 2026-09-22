/// <summary>
/// "Is ANY of these protecting the player right now?"
///
/// PlayerDeath, PlayerHurt and MountHitAbsorber each ask exactly this about the same list - the
/// fairy, the death animation, the recovery window after a hit - and each used to carry its
/// own copy of the loop. One copy now, so a fourth caller costs one line.
/// </summary>
public static class InvincibilityExtensions
{
    public static bool AnyActive(this IInvincible[] sources)
    {
        if (sources == null)
            return false;

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null && sources[i].IsInvincible)
                return true;
        }

        return false;
    }
}
