using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The only thing a weapon needs to know about pooling: ask for one, give it back.
    ///
    /// Kept deliberately tiny (Interface Segregation) - a weapon has no business seeing
    /// prewarm counts, growth policies or the factory behind them.
    /// </summary>
    /// <typeparam name="T">A Component that knows it is pooled.</typeparam>
    public interface IObjectPool<T> where T : Component, IPoolable
    {
        /// <summary>An active, ready-to-use item, or null when the pool is exhausted.</summary>
        T Get();

        /// <summary>Puts the item back to sleep. Releasing twice is safely ignored.</summary>
        void Release(T item);

        /// <summary>How many items are waiting to be reused.</summary>
        int CountInactive { get; }
    }
}
