using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// ROLE: The OWNER's view of a pool: Get plus ReleaseAll (bring every item home).
    /// PATTERNS: Pooling.
    /// SOLID: I - weapons see only IObjectPool; only the owner sees this bigger view.
    ///
    /// A pool that can also be emptied in one go - what the OWNER of a pool needs, as opposed
    /// to what a user of it needs.
    ///
    /// The split is Interface Segregation drawn along a real line. A weapon only ever asks for
    /// an item - the item goes home by itself - so it sees IObjectPool and nothing more: it
    /// cannot reach in and recall everyone else's shots. The pool manager, which is the one object entitled to
    /// say "this level is over, everything comes home", sees this larger view.
    ///
    /// Before it existed, the manager had to hold the CONCRETE GenericObjectPool for the sake
    /// of one method that the interface happened to be missing - a dependency on an
    /// implementation caused purely by a gap in the abstraction (Dependency Inversion).
    /// </summary>
    /// <typeparam name="T">A Component that knows it is pooled.</typeparam>
    public interface IFlushablePool<T> : IObjectPool<T> where T : Component, IPoolable
    {
        /// <summary>Takes back every item that is still out. Releasing an idle pool is a no-op.</summary>
        void ReleaseAll();
    }
}
