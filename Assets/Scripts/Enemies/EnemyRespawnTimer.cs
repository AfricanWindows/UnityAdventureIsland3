using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core;
using UnityEngine;

/// <summary>
/// "This enemy comes back N seconds after it is beaten, in the place where it fell."
///
/// Drop it on an enemy, set one number, done. It is a separate component and not a field
/// inside BaseEnemy on purpose: an enemy that should stay dead simply does not carry it,
/// and nothing about respawning leaks into the class that only knows how to be hurt
/// (Single Responsibility, Open/Closed).
///
/// It talks to IRespawnable, so it never names BaseEnemy and would work just as well on a
/// breakable crate (Dependency Inversion).
///
/// WHY ASYNC AND NOT A COROUTINE - this is the interesting part. A beaten enemy is
/// SetActive(false), and Unity kills the coroutines of a disabled object on the spot, so a
/// coroutine countdown would be cancelled by the very event that starts it. Task.Delay is
/// not driven by the player loop, so it keeps counting while the object sleeps. The
/// mechanic genuinely requires async here; it is not async for the sake of a checkbox.
///
/// The token is the price of that: a Task that nobody cancels also survives leaving Play
/// Mode, and the next session would start with a respawn already pending.
/// </summary>
[DisallowMultipleComponent]
public class EnemyRespawnTimer : MonoBehaviour, IResettable
{
    [Tooltip("Seconds between being beaten and coming back. 0 or less = never comes back.")]
    [SerializeField] private float respawnDelaySeconds = 5f;

    [Tooltip("Log every countdown. Handy while balancing, noisy otherwise.")]
    [SerializeField] private bool verbose;

    private IRespawnable target;
    private CancellationTokenSource cts;

    private void Awake()
    {
        target = GetComponent<IRespawnable>();

        if (target == null)
            Debug.LogError("EnemyRespawnTimer: nothing on " + gameObject.name +
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

        cts = new CancellationTokenSource();
        _ = RespawnAfterDelayAsync(cts.Token);
    }

    private async Task RespawnAfterDelayAsync(CancellationToken token)
    {
        try
        {
            if (verbose)
                Debug.Log("[Respawn] " + gameObject.name + " returns in " + respawnDelaySeconds + "s");

            await Task.Delay((int)(respawnDelaySeconds * 1000f), token);

            // Checked again after the wait: several seconds is long enough for the object
            // to have been destroyed, or for a restart to have brought it back already.
            if (token.IsCancellationRequested || this == null || target == null)
                return;

            if (target.IsDefeated)
                target.Revive();
        }
        catch (OperationCanceledException)
        {
            // The normal way this ends - a restart, or leaving Play Mode. Not an error,
            // so it is not logged as one.
        }
    }

    private void Cancel()
    {
        if (cts == null)
            return;

        cts.Cancel();
        cts.Dispose();
        cts = null;
    }
}
