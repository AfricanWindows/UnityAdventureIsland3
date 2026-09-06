using Game.Core;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The axe thrower. It decides WHEN an axe may leave Mario's hand - nothing else.
    ///
    /// Two things were taken away from the old version. The Instantiate/Destroy cycle went
    /// to the pool, and the ammo count went to AmmoMagazine, a plain C# class this weapon
    /// merely owns. What is left is one job: "the trigger was pulled, is a throw allowed,
    /// where does it start" (Single Responsibility).
    ///
    /// It still exposes AddAmmo and Reload, so AxeAmmoPowerUp needed no change at all, and
    /// it still registers a counter with CounterRegistry, so the HUD label needed no change
    /// either - the counter it registers is now the magazine rather than the weapon itself.
    /// </summary>
    public sealed class AxeWeapon : BaseWeapon, IReloadWeapon
    {
        [Tooltip("The pool that hands out axes. Drag the AxePool object here.")]
        [SerializeField] private AxePoolManager axePool;

        [Tooltip("Where an axe appears. Empty = this object's own position.")]
        [SerializeField] private Transform firePoint;

        [Tooltip("How many axes Mario starts the level with")]
        [SerializeField] private int startAmmo = 0;

        private IObjectPool<ProjectileAxe> _pool;
        private Transform _firePoint;
        private IFacing _facing;
        private AmmoMagazine _magazine;

        public override WeaponType Type { get { return WeaponType.Axe; } }

        /// <summary>
        /// Unlocked, off cooldown, AND holding at least one axe. The base class supplies
        /// the first two; the only rule this weapon adds is the ammo one (Open/Closed).
        /// </summary>
        public override bool CanFire
        {
            get { return base.CanFire && _magazine != null && _magazine.HasRounds; }
        }

        protected override void OnAwake()
        {
            _magazine = new AmmoMagazine(startAmmo);

            _pool = axePool;
            _firePoint = firePoint != null ? firePoint : transform;
            _facing = GetComponentInParent<IFacing>();

            // The axe is never "locked" behind a power-up - running out of axes is its
            // whole limit - so it unlocks itself instead of relying on an Inspector tick
            // that nobody would remember to set.
            Equip();

            if (_pool == null)
                Debug.LogError("[Axe] AxeWeapon has no AxePoolManager assigned.", this);
        }

        private void OnEnable()
        {
            // Tell the UI where to find this counter (see CounterRegistry).
            CounterRegistry.Register(CounterId.Axes, _magazine);
            _magazine.Raise();
        }

        private void OnDisable()
        {
            CounterRegistry.Unregister(CounterId.Axes, _magazine);
        }

        protected override bool FireInternal()
        {
            if (_pool == null)
                return false;

            ProjectileAxe axe = _pool.Get();

            if (axe == null)
            {
                Debug.Log("[Axe] Pool exhausted - all axes are still in the air");
                return false;
            }

            // Ammo is spent only once an axe really left the pool, so an exhausted pool
            // never eats a round.
            if (!_magazine.TrySpend(1))
            {
                axe.Despawn();
                return false;
            }

            axe.Launch(_firePoint.position, _facing != null ? _facing.FacingDirection : 1f);
            return true;
        }

        /// <summary>Used by AxeAmmoPowerUp when Mario picks axes up.</summary>
        public void AddAmmo(int amount)
        {
            _magazine.Add(amount);
        }

        public void Reload()
        {
            AddAmmo(1);
        }
    }
}
