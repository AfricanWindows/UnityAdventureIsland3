using UnityEngine;

/// <summary>
/// Shows a TimedPlayerEffect by tinting a sprite. Pure presentation:
/// it never decides when the effect starts or ends, it only listens.
/// Works with ANY TimedPlayerEffect - one view component per effect you want to show.
/// </summary>
public class TimedEffectView : MonoBehaviour
{
    [SerializeField] private TimedPlayerEffect effect;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color activeColor = Color.red;

    private Color normalColor = Color.white;

    private void Awake()
    {
        if (effect == null)
            effect = GetComponent<TimedPlayerEffect>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            normalColor = targetRenderer.color;
    }

    private void OnEnable()
    {
        if (effect == null)
        {
            Debug.LogWarning("TimedEffectView: no TimedPlayerEffect found on " + gameObject.name, this);
            return;
        }

        effect.OnActiveChanged += ShowState;
        ShowState(effect.IsActive);
    }

    private void OnDisable()
    {
        if (effect != null)
            effect.OnActiveChanged -= ShowState;
    }

    private void ShowState(bool isActive)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.color = isActive ? activeColor : normalColor;
    }
}
