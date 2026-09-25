using UnityEngine;

/// <summary>
/// ROLE: View - blinks the sprite while the recovery window is open.
/// PATTERNS: MVC-style View.
/// SOLID: D - reads IHitRecovery only.
///
/// VIEW of the recovery window: the sprite blinks between full and half transparency while
/// the IHitRecovery window is open, so the player can SEE that he is untouchable for a moment.
///
/// It only reads a bool and changes one number - the sprite's alpha. It never decides when
/// the window starts or ends, and it never names the class that owns it (Dependency
/// Inversion). Only the alpha is touched, never the colour, so the sprite always comes back
/// exactly as it was drawn.
///
/// Update and not a coroutine: the blink is a picture of a state that can start, restart or
/// end at any moment, and polling that state every frame is simpler and safer than keeping
/// a coroutine in step with it. Time.time drives the rhythm, so a paused game freezes it.
/// </summary>
[DisallowMultipleComponent]
public class HitInvincibilityView : MonoBehaviour
{
    [Tooltip("The sprite that blinks. Empty = the first active one on this object or below it.")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Tooltip("Alpha in the faded half of each blink. 1 = no blink at all, 0 = invisible.")]
    [Range(0f, 1f)]
    [SerializeField] private float fadedAlpha = 0.5f;

    [Tooltip("Seconds each half of the blink lasts - faded, then full, then faded again.")]
    [Min(0.02f)]
    [SerializeField] private float blinkInterval = 0.1f;

    private IHitRecovery window;
    private float fullAlpha = 1f;
    private bool blinking;

    private void Awake()
    {
        window = GetComponent<IHitRecovery>();

        if (window == null)
            Debug.LogError("HitInvincibilityView: no IHitRecovery on " + gameObject.name +
                           " - there is no window to show. Add a Hit Invincibility.", this);

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            fullAlpha = targetRenderer.color.a;
        else
            Debug.LogError("HitInvincibilityView: no SpriteRenderer on " + gameObject.name +
                           " - there is nothing to blink.", this);
    }

    private void Update()
    {
        if (targetRenderer == null || window == null)
            return;

        if (window.IsInvincible)
        {
            // Two halves per cycle: the first half faded, the second half full.
            bool faded = Mathf.Repeat(Time.time, blinkInterval * 2f) < blinkInterval;
            SetAlpha(faded ? fadedAlpha : fullAlpha);
            blinking = true;
        }
        else if (blinking)
        {
            StopBlinking();
        }
    }

    // Switched off mid-blink (the player object disabled): never leave him half transparent.
    private void OnDisable()
    {
        if (blinking && targetRenderer != null)
            StopBlinking();
    }

    private void StopBlinking()
    {
        SetAlpha(fullAlpha);
        blinking = false;
    }

    private void SetAlpha(float alpha)
    {
        Color color = targetRenderer.color;
        color.a = alpha;
        targetRenderer.color = color;
    }
}
