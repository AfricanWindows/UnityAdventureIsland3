using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Answers one question: where is the player right now?
    ///
    /// The camera, the level flow and anything else that needs him ask THIS instead of
    /// calling GameObject.FindGameObjectWithTag themselves. The difference matters: a Find
    /// inside LateUpdate is a scene-wide search every frame, and four classes doing it are
    /// four places to fix when the player stops being found (Dependency Inversion).
    /// </summary>
    public interface IPlayerProvider
    {
        /// <summary>The player object, or null if none exists yet.</summary>
        GameObject Player { get; }

        /// <summary>Convenience for the common case. Null-safe.</summary>
        Transform PlayerTransform { get; }
    }
}
