using UnityEngine;

/// <summary>
/// TEMPLATE METHOD for every power-up that works by finding ONE thing on the player and
/// asking it to do something: the power bar, the fruit counter, the invincibility, the
/// weapon slot, the saddle.
///
/// Those five used to repeat the same lines - check the player, search his children
/// (switched-off ones included), complain if nothing is there, make one call - and differed
/// only in the type they searched for and in that one call. The type is now the generic
/// parameter and the call is Apply(); the search and the complaint are written here once
/// (Don't Repeat Yourself, Open/Closed).
///
/// TTarget is always an INTERFACE (IPowerWallet, IWeaponSlot...), so a power-up still never
/// learns which class answers it (Dependency Inversion).
/// </summary>
/// <typeparam name="TTarget">The interface this power-up looks for on the player.</typeparam>
public abstract class PlayerComponentPowerUp<TTarget> : IPowerUp where TTarget : class
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        // true = include inactive, so a component parked on a switched-off child still counts.
        TTarget target = player.GetComponentInChildren<TTarget>(true);

        if (target == null)
        {
            Debug.LogError("[PowerUp] No " + typeof(TTarget).Name + " under " + player.name +
                           " - the pickup gives nothing. Check the player prefab.", player);
            return;
        }

        Apply(target, player);
    }

    /// <summary>The one step each power-up writes: what it does to what it found.</summary>
    protected abstract void Apply(TTarget target, GameObject player);
}
