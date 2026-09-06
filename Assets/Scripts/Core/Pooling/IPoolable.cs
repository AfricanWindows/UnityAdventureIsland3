using System;

namespace Game.Core
{
    /// <summary>
    /// Something that can live in a pool: it is told when it wakes up, when it goes back
    /// to sleep, and how to send itself home.
    ///
    /// The last part is the important one. The pool hands every object a callback, so the
    /// object returns itself by invoking a delegate instead of holding a reference to a
    /// LaserPoolManager. A projectile therefore has NO idea which pool owns it, or that
    /// pools exist - it just says "I am done" (Dependency Inversion).
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called right after the object is handed out: reset the state here.</summary>
        void OnSpawned();

        /// <summary>Called right before the object goes back: stop everything here.</summary>
        void OnDespawned();

        /// <summary>Given by the pool once, at creation time.</summary>
        void SetReleaseCallback(Action release);
    }
}
