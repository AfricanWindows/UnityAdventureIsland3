using UnityEngine;

namespace Game.Projectiles
{
    /// <summary>
    /// Where a projectile's numbers actually live: once, in one asset on disk.
    ///
    /// The alternative is fields on the prefab, which means every copy of the laser owns
    /// its own speed and damage, and re-balancing the weapon turns into hunting down
    /// prefabs. With a ScriptableObject the value exists exactly once, designers edit it
    /// without opening a scene, and nothing is duplicated into memory per instance.
    ///
    /// A second laser variant is a second asset - no new code (Open/Closed).
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Weapons/Projectile Config")]
    public class ProjectileConfigSO : ScriptableObject
    {
        [SerializeField] private ProjectileStats stats = new ProjectileStats(12f, 2f, 1, 1f, false);

        [Tooltip("Optional. Leave empty for a plain sprite with no animation.")]
        [SerializeField] private RuntimeAnimatorController animatorController;

        public ProjectileStats Stats { get { return stats; } }
        public RuntimeAnimatorController AnimatorController { get { return animatorController; } }
    }
}
