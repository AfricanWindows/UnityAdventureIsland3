using UnityEngine;

/// <summary>
/// ROLE: View - shows the fairy beside the player while the effect runs.
/// PATTERNS: MVC-style View; Observer - listens to OnActiveChanged.
/// SOLID: O - any timed effect can get a view like this.
///
/// Shows an object for as long as a TimedPlayerEffect is running - the little fairy flying
/// beside the player, and any companion, aura or shield sprite that comes later.
///
/// It is the one view of the fairy's invincibility: the player himself is not recoloured,
/// the fairy beside him IS the sign. It decides nothing - it listens to OnActiveChanged and
/// draws. That is why the fairy needed no change at all in PlayerInvincible, in the power-up,
/// or in the pickable - the effect already announced itself, and this simply started
/// listening (Open/Closed, Single Responsibility).
///
/// The shown object is a plain GameObject, not a sprite, so it may be a single image or a
/// whole animated child with its own Animator and hover - this class neither knows nor cares.
/// </summary>
public class TimedEffectObjectView : MonoBehaviour
{
    [Tooltip("Which effect to follow. Optional - taken from this object. Set it by hand if " +
             "the player ever carries more than one timed effect.")]
    [SerializeField] private TimedPlayerEffect effect;

    [Tooltip("Switched on while the effect runs, off the rest of the time. Usually a child " +
             "object holding the sprite, so it follows the player by itself.")]
    [SerializeField] private GameObject shownWhileActive;

    private void Awake()
    {
        if (effect == null)
            effect = GetComponent<TimedPlayerEffect>();
    }

    private void OnEnable()
    {
        if (effect == null)
        {
            Debug.LogWarning("TimedEffectObjectView: no TimedPlayerEffect found on " +
                             gameObject.name + " - nothing to follow.", this);
            return;
        }

        effect.OnActiveChanged += ShowState;

        // Asked once on top of subscribing: the effect may already be running when this
        // object is switched on, and a view that only listens for CHANGES would miss it.
        ShowState(effect.IsActive);
    }

    private void OnDisable()
    {
        if (effect != null)
            effect.OnActiveChanged -= ShowState;
    }

    private void ShowState(bool isActive)
    {
        if (shownWhileActive != null)
            shownWhileActive.SetActive(isActive);
    }
}
