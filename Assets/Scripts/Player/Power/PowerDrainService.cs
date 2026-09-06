using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The clock behind the power bar: one segment gone every three seconds, forever.
///
/// This is the project's ASYNC &amp; TASKS piece. It is deliberately NOT a coroutine and
/// NOT an Update timer:
///   - a coroutine dies with its MonoBehaviour and cannot be awaited or composed
///   - an Update timer costs an engine call every frame to check a number that only
///     matters once every 180 frames
/// An async loop that awaits Task.Delay costs nothing between ticks.
///
/// It is a plain C# class with no UnityEngine reference, and it does not know what a
/// player is. It ticks an IPowerModel (Dependency Inversion), so the same service drives
/// a boss timer or a bomb fuse without a change.
///
/// THE RULE THAT MATTERS: every wait takes the CancellationToken, and Stop() must be
/// called from OnDisable/OnDestroy. A Task.Delay that nobody cancelled keeps running
/// after you leave Play Mode - Unity does not kill it - and the next Play session then
/// has two timers draining the same bar.
/// </summary>
public class PowerDrainService
{
    private readonly IPowerModel _model;
    private readonly int _intervalMilliseconds;

    private CancellationTokenSource _cts;

    /// <summary>True while the loop is running.</summary>
    public bool IsRunning { get { return _cts != null; } }

    public PowerDrainService(IPowerModel model, float intervalSeconds)
    {
        if (model == null)
            throw new ArgumentNullException("model", "PowerDrainService needs a model to drain.");

        _model = model;

        // Guarded: an interval of zero would be a tight loop that hangs the editor.
        float safeInterval = intervalSeconds > 0.05f ? intervalSeconds : 0.05f;
        _intervalMilliseconds = (int)(safeInterval * 1000f);
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
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_intervalMilliseconds, token);

                // Checked again after the wait: three seconds is long enough for the
                // scene to have been unloaded while we slept.
                if (token.IsCancellationRequested)
                    return;

                // Task.Delay hands control back on the context we started from, which is
                // Unity's main thread - so touching the model, and through it the UI, is
                // safe here. Anything moved onto Task.Run would NOT be.
                _model.Remove(1);
            }
        }
        catch (OperationCanceledException)
        {
            // The normal way this loop ends. Not an error, so it is not logged as one.
        }
    }
}
