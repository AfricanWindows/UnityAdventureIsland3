using Game.Core;
using UnityEngine;

/// <summary>
/// The player's saddle: WHICH animal he is riding, and what that animal costs or gives him.
///
/// He owns ONE animal, a new one replaces it, and that rule is written in Mount() and
/// nowhere else.
///
/// It is a SEPARATE slot from the weapon, which is the whole reason the axe survives a ride:
/// nothing here ever touches IWeaponSlot. While an animal is carried this component answers
/// IAttackOverride with that animal's attack, so the ONE reader of the attack button
/// (WeaponsHandler) fires the animal instead of the axe; the moment the animal is lost the
/// answer goes back to null and the axe has the button again, with no state to restore and
/// nothing to remember.
///
/// It does NOT read the attack button itself, and that is deliberate. It used to, which meant
/// two components polled the same key on the same frame and a flag (the old IAttackLock) had
/// to keep them from both firing. One reader and one question - "whose weapon is it?" - is
/// the same behaviour with nothing left to keep in step (Single Responsibility).
///
/// It is a plain MonoBehaviour and no longer an InputDrivenBehaviour. That base class exists
/// to hand a component the player's IInputSource, and this one stopped reading the button when
/// IAttackOverride replaced its Update - so it was being injected with a device it never asked
/// a single question. A class should not carry a dependency it does not use (Dependency
/// Inversion), and dropping it costs nothing: the base serializes no fields, and PlayerDeath
/// still reaches this component through IPlayerDeathHandler, which it never stopped answering.
///
/// The animal TAKING A HIT for the player is a rule of its own and lives in MountHitAbsorber,
/// which only asks this class through IMountSlot.
/// </summary>
[DisallowMultipleComponent]
public class PlayerMount : MonoBehaviour, IMountSlot, IAttackOverride, IResettable, IPlayerDeathHandler
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

    /// <summary>
    /// While he rides, the button fires the ANIMAL and the weapon in his hand waits.
    ///
    /// An explicit null test rather than "current?.Weapon": behind the interface sits a
    /// Unity Object with its own overloaded ==, which the null-conditional operator does
    /// not honour.
    /// </summary>
    public IUseableWeapon OverrideWeapon
    {
        get { return current != null ? current.Weapon : null; }
    }

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
