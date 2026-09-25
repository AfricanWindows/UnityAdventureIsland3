using Game.Weapons;
using UnityEngine;

/// <summary>
/// ROLE: One rideable animal: its look, the player's size on it, and its attack.
/// PATTERNS: none - the attack is a BaseWeapon it holds (composition).
/// SOLID: O - three animals are three sets of data, not three copies of code.
///
/// ONE animal the player can ride. There are three of them and they differ in DATA, not in
/// behaviour: which clips the player is drawn with, how big he becomes, and which attack
/// answers the button. So there is one class and three sets of numbers, not three classes
/// with copied code.
///
/// It lives as a component ON THE PLAYER, switched off until he finds it - exactly how the
/// axe and the boomerang already live there. That is what makes mounting free of
/// Instantiate, of dependency injection problems and of state transfer: nothing is created
/// or destroyed, a flag simply changes.
///
/// The attack is a BaseWeapon held by REFERENCE rather than implemented here, and that is
/// the important decision. Two of the animals shoot, which is exactly what
/// DirectionalWeapon already does for the axe - pooling, cooldown, facing, the projectile
/// factory, all of it written and tested. C# allows one base class, so an animal that
/// wanted to BE a weapon could not also be a mount; holding one lets every animal reuse
/// the whole weapon machinery and lets the odd one out (the spin) be something else
/// entirely (composition over inheritance).
///
/// The subclasses below it are empty. They exist only so that a pickup can name ONE animal
/// - the same reason AxePickable can name AxeWeapon - and they are what makes
/// MountAnimalPowerUp&lt;T&gt; possible.
/// </summary>
public abstract class AnimalMount : MonoBehaviour
{
    [Header("How the player looks while riding")]
    [Tooltip("Animator Override Controller with this animal's clips. The player's own " +
             "controller and every parameter stay exactly as they are, so PlayerAnimatorView " +
             "needs no change - only the clips behind the states are swapped.")]
    [SerializeField] private RuntimeAnimatorController mountedLook;

    [Header("How big the player becomes")]
    [Tooltip("Radius of the player's circle collider while riding. Leave it at his normal " +
             "radius for an animal that does not change his size.")]
    [SerializeField] private float colliderRadius = 0.5f;

    [Tooltip("Offset of that collider while riding. Grow the shape UPWARDS - a bigger " +
             "radius with the old offset pushes the player's feet into the floor.")]
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;

    [Header("What it attacks with")]
    [Tooltip("The attack component for this animal, sitting on the player next to it. " +
             "Equipped while riding, unequipped the moment the animal is lost.")]
    [SerializeField] private BaseWeapon attack;

    /// <summary>The clips the player is drawn with while riding this one.</summary>
    public RuntimeAnimatorController MountedLook { get { return mountedLook; } }

    public float ColliderRadius { get { return colliderRadius; } }

    public Vector2 ColliderOffset { get { return colliderOffset; } }

    /// <summary>What the attack button fires while riding this one.</summary>
    public IUseableWeapon Weapon { get { return attack; } }

    private void Awake()
    {
        if (attack == null)
            Debug.LogError(GetType().Name + ": no attack assigned - riding it would leave " +
                           "the player unable to attack at all.", this);

        if (mountedLook == null)
            Debug.LogWarning(GetType().Name + ": no Animator Override Controller assigned - " +
                             "the player will keep his own look while riding.", this);
    }
}
