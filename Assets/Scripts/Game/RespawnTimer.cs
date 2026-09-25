using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core;
using UnityEngine;

/// <summary>
/// ROLE: Brings back anything IRespawnable (an enemy, a fruit) N seconds after it was taken out.
/// PATTERNS: Async and Tasks - async/await with a CancellationToken, because the object is
///           switched off and a coroutine would stop; Observer - listens to Defeated.
/// SOLID: S - respawning is its own component; D - talks to IRespawnable only.
///
/// "This comes back N seconds after it was taken out of the game."
///
/// Drop it on anything that implements IRespawnable - an enemy, a fruit, an egg - set one
/// number, done. It is a separate component and not a field inside BaseEnemy on purpose:
/// something that should stay gone simply does not carry it, and nothing about respawning
/// leaks into the classes that only know how to be beaten, eaten or opened (Single
/// Responsibility, Open/Closed). It talks to IRespawnable only, so it never learns WHAT it
/// brings back (Dependency Inversion) - that is why one class serves all three.
///
/// WHY ASYNC AND NOT A COROUTINE - this is the interesting part. A beaten enemy or an eaten
/// fruit is SetActive(false), and Unity stops the coroutines of a disabled object on the
/// spot, so a coroutine countdown would be cancelled by the very event that starts it. An
/// async method is not owned by the GameObject, so it keeps counting while the object
/// sleeps. The mechanic genuinely needs async here; it is not async for a checkbox.
///
/// SAFETY - what keeps this from leaking or firing at the wrong moment:
/// - GAME time, not wall-clock time. It waits frame by frame and adds Time.deltaTime, which
///   is 0 while Time.timeScale is 0, so nothing respawns behind a Game Over panel.
///   (Task.Delay, the old version, kept counting through the pause.)
/// - One countdown at a time. Starting a new one cancels the old one first.
/// - Cancelled by a new game (ResetToStart) and by destruction: the token is LINKED to
///   Unity's destroyCancellationToken, so even leaving Play Mode stops it.
/// - Nothing fails silently. A fire-and-forget Task swallows its exceptions; this one
///   catches them and hands them to the console.
/// </summary>
[DisallowMultipleComponent]
public class RespawnTimer : MonoBehaviour, IResettable
{
    [Tooltip("Seconds between being taken out of the game and coming back. 0 or less = " +
             "never comes back.")]
    [SerializeField] private float respawnDelaySeconds = 5f;

    private IRespawnable target;
    private CancellationTokenSource countdown;

    private void Awake()
    {
        target = GetComponent<IRespawnable>();

        if (target == null)
            Debug.LogError("RespawnTimer: nothing on " + gameObject.name +
                           " implements IRespawnable - there is nothing to bring back.", this);
    }

    // Subscribed in Start and unsubscribed only in OnDestroy - deliberately NOT the usual
    // OnEnable/OnDisable pair. The event we care about is the one that DISABLES this object,
    // and an OnDisable unsubscribe would drop the countdown at the exact moment it starts.
    private void Start()
    {
        if (target != null)
        {
            target.Defeated -= StartCountdown;
            target.Defeated += StartCountdown;
        }
    }

    private void OnDestroy()
    {
        if (target != null)
            target.Defeated -= StartCountdown;

        Cancel();
    }

    /// <summary>A new game: no pending countdown, the restart puts everyone back itself.</summary>
    public void ResetToStart()
    {
        Cancel();
    }

    private void StartCountdown()
    {
        if (respawnDelaySeconds <= 0f)
            return;

        Cancel();

        countdown = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        _ = CountDownAsync(countdown.Token);
    }

    private async Task CountDownAsync(CancellationToken token)
    {
        try
        {
            float elapsed = 0f;

            while (elapsed < respawnDelaySeconds)
            {
                await Awaitable.NextFrameAsync(token);
                elapsed += Time.deltaTime;
            }

            // Asked again after the wait: a restart may have brought it back already.
            if (target.IsDefeated)
                target.Revive();
        }
        catch (OperationCanceledException)
        {
            // The normal way this ends early - a new game, or leaving Play Mode. Not an
            // error, so it is not logged as one.
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    private void Cancel()
    {
        if (countdown == null)
            return;

        countdown.Cancel();
        countdown.Dispose();
        countdown = null;
    }
}
