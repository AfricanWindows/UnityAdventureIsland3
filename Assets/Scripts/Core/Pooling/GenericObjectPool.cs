using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Reuses objects instead of creating and destroying them, which is what keeps the
    /// frame rate flat while Mario holds the fire button: Instantiate/Destroy allocate,
    /// and allocations are what the garbage collector eventually stops the game to clean.
    ///
    /// It is a PLAIN C# class, not a MonoBehaviour: pooling is bookkeeping, not something
    /// that needs a transform, a GameObject or an Update. The MonoBehaviour part lives in
    /// the pool manager, whose only job is to build this object (Single Responsibility).
    ///
    /// It also never creates anything itself - it asks an IFactory. So the pool works for
    /// lasers, fireballs, coins or enemies without a single change (Open/Closed).
    ///
    /// Note: Unity ships UnityEngine.Pool.ObjectPool&lt;T&gt;, which does roughly this.
    /// The exercise asks for our own, so here it is.
    /// </summary>
    public class GenericObjectPool<T> : IObjectPool<T> where T : Component, IPoolable
    {
        private readonly IFactory<T> _factory;
        private readonly int _maxSize;
        private readonly bool _allowGrowth;

        // Queue, not List: Enqueue/Dequeue are O(1). Looking for a free object by walking
        // a List and testing activeInHierarchy costs O(n) on EVERY shot - the exact thing
        // that makes rapid fire get slower the bigger the pool grows.
        private readonly Queue<T> _inactive;

        // HashSet, not List: Remove/Contains are O(1), which is how "was this item really
        // handed out?" is answered cheaply. That check is what stops a double Release from
        // putting the same laser into the queue twice and handing it to two shots at once.
        private readonly HashSet<T> _active;

        private int _totalCreated;

        /// <summary>Raised after an item was handed out. Lets a manager log or count
        /// without the pool itself knowing what a "laser" is.</summary>
        public event Action<T> ItemTaken;

        /// <summary>Raised after an item really went back (a double release raises nothing).</summary>
        public event Action<T> ItemReleased;

        public int CountInactive { get { return _inactive.Count; } }
        public int CountActive { get { return _active.Count; } }

        /// <summary>Everything this pool ever created, including what is in flight.</summary>
        public int CountAll { get { return _totalCreated; } }

        /// <param name="factory">Where new items come from. The only dependency.</param>
        /// <param name="prewarmCount">How many to create up front, before the first shot.</param>
        /// <param name="maxSize">Hard ceiling on how many items may ever exist.</param>
        /// <param name="allowGrowth">May the pool create more than it prewarmed?</param>
        public GenericObjectPool(IFactory<T> factory, int prewarmCount, int maxSize, bool allowGrowth)
        {
            if (factory == null)
                throw new ArgumentNullException("factory", "GenericObjectPool cannot work without a factory.");

            _factory = factory;
            _maxSize = Mathf.Max(1, maxSize);
            _allowGrowth = allowGrowth;

            int toPrewarm = Mathf.Clamp(prewarmCount, 0, _maxSize);

            // Sized once, so filling them never reallocates the backing arrays.
            _inactive = new Queue<T>(toPrewarm);
            _active = new HashSet<T>();

            Prewarm(toPrewarm);
        }

        /// <summary>
        /// Pays the Instantiate cost during loading, where a hitch is invisible, instead of
        /// during the first shot, where it is not.
        /// </summary>
        /// <returns>How many items were actually created.</returns>
        public int Prewarm(int count)
        {
            int created = 0;

            for (int i = 0; i < count; i++)
            {
                T item = CreateNew();
                if (item == null)
                    break;

                item.gameObject.SetActive(false);
                _inactive.Enqueue(item);
                created++;
            }

            return created;
        }

        /// <summary>
        /// Hands out a ready item, or null when the pool is empty and not allowed to grow.
        /// Callers must handle null - the whole point of a ceiling is that it is reached.
        /// </summary>
        public T Get()
        {
            T item = TakeFromQueue();

            if (item == null)
            {
                // A pool with growth switched off is MEANT to run dry - that is how "five
                // shots in the air at once" is expressed - so it says nothing and lets the
                // caller decide what an empty pool means. Hitting maxSize while growth was
                // allowed is different: that one is a capacity problem worth reporting.
                if (!_allowGrowth)
                    return null;

                if (_totalCreated >= _maxSize)
                {
                    Debug.LogWarning("[Pool] " + typeof(T).Name + " reached its ceiling (" +
                                     _totalCreated + "/" + _maxSize + ") - nothing to hand out.");
                    return null;
                }

                item = CreateNew();
                if (item == null)
                    return null;
            }

            _active.Add(item);
            item.gameObject.SetActive(true);
            item.OnSpawned();

            if (ItemTaken != null)
                ItemTaken(item);

            return item;
        }

        /// <summary>
        /// Takes the item back. Called by the item itself through the release callback,
        /// so nothing outside the pool has to remember where an object belongs.
        /// </summary>
        public void Release(T item)
        {
            if (item == null)
                return;

            // O(1), and it doubles as the guard: an item that is not in _active was either
            // never handed out or is already back. Either way, putting it in the queue
            // again would hand the same object to two shots at once.
            if (!_active.Remove(item))
            {
                Debug.LogWarning("[Pool] " + item.name + " was released twice - ignored.", item);
                return;
            }

            item.OnDespawned();
            item.gameObject.SetActive(false);
            _inactive.Enqueue(item);

            if (ItemReleased != null)
                ItemReleased(item);
        }

        /// <summary>
        /// Next living item from the queue, skipping any that Unity destroyed behind our
        /// back (a scene change wipes the objects but not this list).
        /// </summary>
        private T TakeFromQueue()
        {
            while (_inactive.Count > 0)
            {
                T candidate = _inactive.Dequeue();

                // Unity overloads ==, so this also catches a destroyed-but-not-null object.
                if (candidate != null)
                    return candidate;

                _totalCreated--;
            }

            return null;
        }

        private T CreateNew()
        {
            T item = _factory.Create();

            if (item == null)
            {
                Debug.LogError("[Pool] Factory produced no " + typeof(T).Name + " - check the prefab and the config.");
                return null;
            }

            _totalCreated++;

            // The item is told HOW to go home, not WHERE home is. It captures this call,
            // never a LaserPoolManager, so a projectile stays usable with any pool - or
            // with no pool at all (Dependency Inversion).
            item.SetReleaseCallback(delegate { Release(item); });

            return item;
        }
    }
}
