using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Finds the player by TAG, once, and remembers him.
    ///
    /// A plain C# class, not a MonoBehaviour: looking something up is not behaviour that
    /// needs a transform or an Update. GameInstaller creates it and registers it as
    /// IPlayerProvider; nothing else ever names this type, so swapping "find by tag" for
    /// "the installer holds a direct reference" is a one-line change there (Open/Closed).
    ///
    /// The search is LAZY and cached. Lazy, because the installer may run before the
    /// player is enabled; cached, because Unity's overloaded == turns a destroyed player
    /// back into null and the lookup then simply happens again.
    /// </summary>
    public class TaggedPlayerProvider : IPlayerProvider
    {
        private readonly string _tag;
        private GameObject _cached;

        public TaggedPlayerProvider(string playerTag)
        {
            _tag = string.IsNullOrEmpty(playerTag) ? "Player" : playerTag;
        }

        public GameObject Player
        {
            get
            {
                // Unity's == is overloaded, so a destroyed object reads as null here and
                // the search runs again instead of handing out a corpse.
                if (_cached == null)
                    _cached = GameObject.FindGameObjectWithTag(_tag);

                return _cached;
            }
        }

        public Transform PlayerTransform
        {
            get
            {
                GameObject player = Player;
                return player != null ? player.transform : null;
            }
        }
    }
}
