using Game.Projectiles;
using UnityEngine;

namespace Game.Weapons
{
    /// <summary>
    /// The boomerang's pool. Like the axe's, it exists because Unity cannot show a
    /// generic MonoBehaviour in the Inspector - but this one does add something real: the
    /// pool is sized to ONE, and that single pooled boomerang IS the round of ammo.
    /// Throwing empties the pool, so Get() returns null until the boomerang finishes its
    /// loop and comes home, and the two console messages say exactly that.
    ///
    /// Overriding the two log hooks rather than copying the pool logic is the point:
    /// the behaviour that differs is the only thing written here (Open/Closed).
    /// </summary>
    public class BoomerangPoolManager : ProjectilePoolManager<BoomerangProjectile>
    {
        protected override void OnItemTaken(BoomerangProjectile item)
        {
            Debug.Log("[Boomerang] Thrown - no round left until it returns");
        }

        protected override void OnItemReleased(BoomerangProjectile item)
        {
            Debug.Log("[Boomerang] Returned to pool - ready to throw again");
        }
    }
}
