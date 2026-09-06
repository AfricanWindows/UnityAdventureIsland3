using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for any effect that turns ON, lasts for a while and turns OFF by itself
/// (invincibility today, shield or speed boost tomorrow).
/// The timer logic is written here ONCE - a child class only gives it a meaning.
/// Knows nothing about how the effect LOOKS: it only reports state through OnActiveChanged.
/// </summary>
public abstract class TimedPlayerEffect : MonoBehaviour
{
    [SerializeField] private float duration = 5f;

    private Coroutine running;

    private bool isActive;

    public bool IsActive
    {
        get { return isActive; }
    }

    /// <summary>Raised with true when the effect starts, false when it ends.</summary>
    public event Action<bool> OnActiveChanged;

    /// <summary>
    /// Starts the effect. Collecting a second one while the first is still running
    /// restarts the full duration instead of leaving two timers fighting each other.
    /// </summary>
    public void Activate()
    {
        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(RunEffect());
    }

    public void Deactivate()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }

        SetActiveState(false);
    }

    private IEnumerator RunEffect()
    {
        SetActiveState(true);

        yield return new WaitForSeconds(duration);

        running = null;
        SetActiveState(false);
    }

    private void SetActiveState(bool value)
    {
        if (isActive == value)
            return;

        isActive = value;

        if (OnActiveChanged != null)
            OnActiveChanged(value);
    }

    private void OnDisable()
    {
        // Unity kills coroutines on disable - without this the effect would stay ON forever.
        Deactivate();
    }
}
