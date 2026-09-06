using System;
using System.Collections.Generic;

/// <summary>
/// Small lookup table for ICounter sources.
///
/// Mario is created by the Level Creator, so he does not exist when the UI wakes up:
/// a UI label simply CANNOT hold a direct reference to his components.
/// Instead every counter registers itself here, and every view asks for a counter by id
/// and gets notified the moment a matching one appears.
///
/// The UI still depends only on the ICounter abstraction, never on a concrete class.
/// </summary>
public static class CounterRegistry
{
    private static readonly Dictionary<CounterId, ICounter> counters = new Dictionary<CounterId, ICounter>();

    /// <summary>Raised when a counter appears (or is replaced by a newly spawned one).</summary>
    public static event Action<CounterId, ICounter> OnCounterRegistered;

    public static void Register(CounterId id, ICounter counter)
    {
        if (counter == null)
            return;

        counters[id] = counter;

        if (OnCounterRegistered != null)
            OnCounterRegistered(id, counter);
    }

    public static void Unregister(CounterId id, ICounter counter)
    {
        ICounter existing;
        if (!counters.TryGetValue(id, out existing))
            return;

        // A newer source already took this slot - do not erase it.
        if (existing != counter)
            return;

        counters.Remove(id);
    }

    public static ICounter Get(CounterId id)
    {
        ICounter counter;
        counters.TryGetValue(id, out counter);
        return counter;
    }
}
