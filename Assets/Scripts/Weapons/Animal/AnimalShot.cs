using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// ROLE: The red and the blue animals' shot.
    /// PATTERNS: Pooling - lives in the animal shot pool; Template Method - inherits the Fire skeleton.
    ///
    /// What the shooting animals spit - the red one's fire and the blue one's shot, both
    /// out of ONE pool.
    ///
    /// Almost everything is inherited: it is a DirectionalProjectile, so the flight, the
    /// facing, the lifetime, the pool handshake and the shared IDamageable hit rule are all
    /// already written. Hitting through IDamageable is exactly the rule the assignment asks
    /// for - it kills enemies and breaks stones (which carry BreakableByAttacks), while the
    /// campfire has no IDamageable and so cannot be shot out, only ridden through.
    ///
    /// The one thing it adds is SetSprite. Two animals shoot different-looking shots, and a
    /// pool hands out copies of ONE prefab - so instead of a second type, a second pool and a
    /// second prefab for what is otherwise the same projectile, the weapon dresses the shot
    /// as it leaves. The alternative (two whole chains, as the axe and the boomerang have) is
    /// the right answer when two projectiles BEHAVE differently; here they differ in one
    /// image.
    /// </summary>
    public sealed class AnimalShot : DirectionalProjectile
    {
        // Found on first use and kept. A pooled object is reused for the whole game, so this
        // search happens once per projectile in the pool, not once per shot.
        private SpriteRenderer _view;

        /// <summary>
        /// Whose shot this is, decided by the weapon that fired it. A null sprite leaves the
        /// prefab's own image alone, so a weapon that does not care simply says nothing.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (_view == null)
                _view = GetComponentInChildren<SpriteRenderer>(true);

            if (_view != null)
                _view.sprite = sprite;
        }
    }
}
