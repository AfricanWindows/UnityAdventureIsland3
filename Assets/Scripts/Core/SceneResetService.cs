using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Finds every IResettable in the scene at the moment of the reset and resets it.
    /// The search happens at call time on purpose: objects created during play
    /// (dropped items) must be reset too, and a list cached at start-up would miss them.
    /// GameInstaller creates it and registers it as IResetService; nothing else names this type.
    ///
    /// A plain C# class, not a MonoBehaviour - the same shape as TaggedPlayerProvider. Finding
    /// things is not behaviour that needs a transform or an Update (Single Responsibility).
    /// </summary>
    public class SceneResetService : IResetService
    {
        /// <summary>
        /// Inactive objects are included: level two is switched off during level one, and its
        /// enemies still have to be reset. It runs only on a restart - never per frame.
        /// </summary>
        public int ResetAll()
        {
            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            List<IResettable> targets = new List<IResettable>();

            for (int i = 0; i < all.Length; i++)
            {
                IResettable resettable = all[i] as IResettable;

                if (resettable != null)
                    targets.Add(resettable);
            }

            // Collected first, then called: a ResetToStart that switches an object back on
            // must not disturb the array we are walking.
            for (int i = 0; i < targets.Count; i++)
                targets[i].ResetToStart();

            return targets.Count;
        }
    }
}
