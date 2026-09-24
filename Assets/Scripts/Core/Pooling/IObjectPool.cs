using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The only thing a weapon needs to know about pooling: ask for one.
    ///
    /// Kept deliberately tiny (Interface Segregation) - a weapon has no business seeing
    /// prewarm counts, growth policies or the factory behind them.
    ///
    /// There is no Release here, and that is not a gap. Giving an item back is not the
    /// borrower's job: the item sends ITSELF home through the callback the pool handed it
    /// (IPoolable.SetReleaseCallback). A Release on this interface was once there, and not
    /// one borrower ever called it.
    /// </summary>
    /// <typeparam name="T">A Component that knows it is pooled.</typeparam>
    public interface IObjectPool<T> where T : Component, IPoolable
    {
        /// <summary>An active, ready-to-use item, or null when the pool is exhausted.</summary>
        T Get();
    }
}
