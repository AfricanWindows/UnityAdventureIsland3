using Game.Core;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// The player's saddle: which animal he is riding, and the trigger finger that fires it.
///
/// It is the twin of WeaponsHandler, deliberately - the same shape for the same job, so
/// there is one idea to explain and not two. He owns ONE animal, a new one replaces it, and
/// that rule is written in Mount() and nowhere else.
///
/// It is a SEPARATE slot from the weapon, which is the whole reason the axe survives a ride:
/// nothing here ever touches IWeaponSlot. While an animal is carried this component answers
/// IAttackLock, so WeaponsHandler holds its fire and the same button reaches the animal
/// instead; the moment the animal is lost the axe answers the button again, with no state to
/// restore and nothing to remember.
///
/// Two jobs - hold and fire - the two halves of "the animal I am riding", exactly like
/// WeaponsHandler's slot and trigger. The animal TAKING A HIT for the player is a rule of its
/// own and lives in MountHitAbsorber, which only asks this class through IMountSlot
/// (Single Responsibility).
/// </summary>
[DisallowMultipleComponent]
public class PlayerMount : InputDrivenBehaviour, IMountSlot, IAttackLock, IResettable, IPlayerDeathHandler
{
    [Tooltip("The player's Animator. Empty = the one on this object.")]
    [SerializeField] private Animator animator;

    [Tooltip("The player's body collider, resized while riding. Empty = the circle collider " +
             "on this object.")]
    [SerializeField] private CircleCollider2D bodyCollider;

    private AnimalMount current;

    // What the player looks like and how big he is on foot. Captured once, so dismounting
    // never has to be told what to go back to.
    private RuntimeAnimatorController footLook;
    private float footRadius;
    private Vector2 footOffset;

    public bool IsMounted { get { return current != null; } }

    /// <summary>While he rides, the weapon in his hand waits.</summary>
    public bool BlocksAttack { get { return IsMounted; } }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<CircleCollider2D>();

        if (animator != null)
            footLook = animator.runtimeAnimatorController;

        if (bodyCollider != null)
        {
            footRadius = bodyCollider.radius;
            footOffset = bodyCollider.offset;
        }
    }

    /// <summary>
    /// Dying costs the animal. Called by PlayerDeath directly, so it arrives even though
    /// this component is switched off for the length of the death animation - the old
    /// static-event version had to subscribe in Start and unsubscribe in OnDestroy to
    /// survive that.
    /// </summary>
    public void OnPlayerDied()
    {
        Dismount();
    }

    private void Update()
    {
        if (!IsMounted || !HasInput)
            return;

        if (!InputSource.AttackPressed)
            return;

        // Whether the shot is allowed - cooldown, an empty pool - is the attack's own
        // business, answered behind Attack() exactly as it is for a weapon.
        if (current.Weapon != null)
            current.Weapon.Attack();
    }

    // ===================== IMountSlot =====================

    public void Mount(AnimalMount animal)
    {
        if (animal == null)
            return;

        if (ReferenceEquals(animal, current))
            return;

        // The old one is let go first, so its attack can never stay armed under the new one.
        if (current != null)
            ReleaseCurrent();

        current = animal;
        ApplyLook(animal.MountedLook);
        ApplyCollider(animal.ColliderRadius, animal.ColliderOffset);

        if (current.Weapon != null)
            current.Weapon.Equip();
    }

    public void Dismount()
    {
        if (current == null)
            return;

        ReleaseCurrent();

        ApplyLook(footLook);
        ApplyCollider(footRadius, footOffset);
    }

    // ===================== IResettable =====================

    /// <summary>A new game starts him on his own two feet. The recovery window resets itself.</summary>
    public void ResetToStart()
    {
        Dismount();
    }

    // =======================================================

    private void ReleaseCurrent()
    {
        if (current.Weapon != null)
            current.Weapon.UnEquip();

        current = null;
    }

    private void ApplyLook(RuntimeAnimatorController look)
    {
        if (animator != null && look != null)
            animator.runtimeAnimatorController = look;
    }

    private void ApplyCollider(float radius, Vector2 offset)
    {
        if (bodyCollider == null)
            return;

        bodyCollider.radius = radius;
        bodyCollider.offset = offset;
    }
}
