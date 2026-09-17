using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The green animal's attack: it spins on the spot and everything within a wide circle
    /// around the player is hit at once. No projectile, no direction, no pool - which is
    /// exactly why it is its own class and not another DirectionalWeapon.
    ///
    /// It is still a BaseWeapon, so it inherits the parts that have nothing to do with
    /// shooting: the "am I equipped" gate that keeps it silent until the player is riding
    /// the green animal, the cooldown, and the Attack() template whose order no subclass can
    /// change. Only the one step that is genuinely different is written here (Template
    /// Method). That is also what lets PlayerMount treat all three animals identically - it
    /// holds a BaseWeapon and never learns that one of them is not a shooter.
    ///
    /// It damages through IDamageable, the same door the axe and the animals' shots use, so
    /// it kills enemies and breaks stones while leaving the campfire - which has no
    /// IDamageable - standing. The assignment's rule falls out of the choice of interface
    /// instead of being written as a list of exceptions.
    /// </summary>
    public sealed class SpinAttack : BaseWeapon
    {
        [Tooltip("How far the spin reaches, in units. This is the BIG radius - the green " +
                 "animal clears the ground around the player rather than hitting ahead.")]
        [SerializeField] private float radius = 2.5f;

        [Tooltip("Damage dealt to each thing caught in the circle. 1 is enough for every " +
                 "enemy in the game.")]
        [SerializeField] private int damage = 1;

        [Tooltip("Which layers the spin can touch. Leave it at Everything unless the level " +
                 "starts hitting things it should not.")]
        [SerializeField] private LayerMask hitLayers = ~0;

        [Tooltip("Largest number of things one spin can hit. Fixed so the attack never " +
                 "allocates while the game runs.")]
        [SerializeField] private int maxTargets = 16;

        // Filled and refilled by Physics2D instead of being allocated per spin. A new array
        // on every attack would be garbage the collector eventually stops the game to clean.
        private Collider2D[] _hits;
        private ContactFilter2D _filter;

        protected override void OnAwake()
        {
            _hits = new Collider2D[Mathf.Max(1, maxTargets)];

            _filter = new ContactFilter2D();
            _filter.useTriggers = true;
            _filter.SetLayerMask(hitLayers);
        }

        /// <summary>
        /// The one step this weapon defines for itself. It reports true even when the circle
        /// was empty: the animal really did spin, and the cooldown should follow the
        /// animation rather than whether anything happened to be standing there.
        /// </summary>
        protected override bool FireInternal()
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, _filter, _hits);

            for (int i = 0; i < count; i++)
            {
                if (_hits[i] == null)
                    continue;

                // InParent: an enemy's collider is often a child of the object that owns its
                // health, and a stone's is not - asking upwards covers both.
                IDamageable target = _hits[i].GetComponentInParent<IDamageable>();

                if (target != null)
                    target.TakeDamage(damage);
            }

            return true;
        }

        /// <summary>Draws the reach in the Scene view, so it can be tuned by eye.</summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
