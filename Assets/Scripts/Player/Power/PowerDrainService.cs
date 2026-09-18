using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The clock behind the power bar: one segment gone every three seconds, forever.
///
/// This is the project's ASYNC &amp; TASKS piece. It is deliberately NOT a coroutine: a
/// coroutine dies with its MonoBehaviour and cannot be awaited or composed. This is a plain
/// C# class with one async loop that the controller starts and cancels.
///
/// It counts GAME time, not wall-clock time. It used to await Task.Delay, which knows
/// nothing about Time.timeScale - so while the Game Over or Level Complete panel had the game
/// frozen, the bar went on draining behind it. After ~45 seconds on the panel the player
/// "died" there, the death waited frozen, and the moment Restart was pressed it finished and
/// took a life from the brand-new game. Adding up Time.deltaTime - which is 0 while the game
/// is frozen - makes the clock stop exactly when the game stops, with no pause logic here.
///
/// It does not know what a player is. It ticks an IPowerModel (Dependency Inversion), so the
/// same service drives a boss timer or a bomb fuse without a change.
///
/// THE RULE THAT MATTERS: every wait takes the CancellationToken, and Stop() must be called
/// from OnDisable/OnDestroy. An async loop that nobody cancelled keeps running after you
/// leave Play Mode - Unity does not kill it - and the next Play session then has two clocks
/// draining the same bar.
/// </summary>
public class PowerDrainService
{
    private readonly IPowerModel _model;
    private readonly float _intervalSeconds;

    private CancellationTokenSource _cts;

    public PowerDrainService(IPowerModel model, float intervalSeconds)
    {
        if (model == null)
            throw new ArgumentNullException("model", "PowerDrainService needs a model to drain.");

        _model = model;

        // Guarded: an interval of zero would take a segment every frame.
        _intervalSeconds = intervalSeconds > 0.05f ? intervalSeconds : 0.05f;
    }

    /// <summary>Starts draining. Calling it twice does not start a second loop.</summary>
    public void Start()
    {
        Stop();

        _cts = new CancellationTokenSource();

        // Fire and forget, but never token-less: the handle we keep is the token source,
        // which is the only thing that can stop it.
        _ = DrainLoopAsync(_cts.Token);
    }

    /// <summary>Stops draining and releases the token source.</summary>
    public void Stop()
    {
        if (_cts == null)
            return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    private async Task DrainLoopAsync(CancellationToken token)
    {
        try
        {
            float elapsed = 0f;

            while (true)
            {
                // One frame. Unity's Awaitable resumes on the main thread and throws
                // OperationCanceledException the moment the token is cancelled, so touching
                // the model - and through it the UI - is safe here.
                await Awaitable.NextFrameAsync(token);

                // SCALED time: 0 while a panel has the game frozen, so the bar waits too.
                elapsed += Time.deltaTime;

                if (elapsed < _intervalSeconds)
                    continue;

                elapsed -= _intervalSeconds;
                _model.Remove(1);
            }
        }
        catch (OperationCanceledException)
        {
            // The normal way this loop ends. Not an error, so it is not logged as one.
        }
    }
}
