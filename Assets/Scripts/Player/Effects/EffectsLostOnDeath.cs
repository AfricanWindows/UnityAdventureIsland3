using UnityEngine;

/// <summary>
/// ROLE: Game rule: dying costs you the fairy.
/// PATTERNS: Observer-style - an IPlayerDeathHandler.
/// SOLID: O - a rule added as a component.
///
/// One rule of the game, written once: dying costs you the fairy.
///
/// It is the twin of WeaponsLostOnDeath - same shape, same reasoning, the other half of
/// "what the player loses when he falls". Dying with a fairy still counting down used to
/// leave him standing at the start of the level red and untouchable, holding a prize he had
/// already paid for.
///
/// It switches off every TimedPlayerEffect it can find, so it covers the fairy and
/// any effect added tomorrow without naming one of them (Open/Closed). Each effect then
/// announces its own ending, and the tint and the sprite disappear by themselves - this
/// class never touches a renderer.
///
/// A SEPARATE component on purpose. The alternative - the effect being a death handler
/// itself - would make "how a timed effect works" depend on "how dying works", which is not
/// its business; TimedPlayerEffect stays usable on anything, player or not. The restart case
/// is different and is NOT here: putting yourself back to your starting state is something
/// the effect knows how to do alone, so that one is its own ResetToStart (Single
/// Responsibility).
/// </summary>
[DisallowMultipleComponent]
public class EffectsLostOnDeath : MonoBehaviour, IPlayerDeathHandler
{
    // Found once, in Awake. A death is the worst possible moment for a component search,
    // and the player's effects never move.
    private TimedPlayerEffect[] effects;

    private void Awake()
    {
        // true = include inactive, so an effect parked on a switched-off child is found too.
        effects = GetComponentsInChildren<TimedPlayerEffect>(true);

        if (effects.Length == 0)
            Debug.LogWarning("[Death] No TimedPlayerEffect under " + name +
                             " - there is nothing for this component to take away.", this);
    }

    /// <summary>Called by PlayerDeath once he is back at the start: the fairy is gone.</summary>
    public void OnPlayerDied()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] == null || !effects[i].IsActive)
                continue;

            effects[i].Deactivate();
        }
    }
}
