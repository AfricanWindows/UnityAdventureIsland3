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
/// It is also the player's IHitAbsorber. Anything that would hurt him - a stone, a campfire,
/// an enemy, a shot - asks first whether something can take the hit; the animal takes it,
/// disappears, and the player walks on with a full power bar. The obstacle that hit him is
/// smashed in the same breath, because the absorber is the only place that knows BOTH what
/// hit him and that an animal was spent.
///
/// Three jobs in one class - hold, fire, absorb - and they are three halves of one idea,
/// "the animal I am riding". WeaponsHandler is built the same way: slot, trigger and reset.
/// </summary>
[DisallowMultipleComponent]
public class PlayerMount : InputDrivenBehaviour, IMountSlot, IAttackLock, IHitAbsorber, IInvincible, IResettable
{
    [Tooltip("Seconds the player cannot be touched after his animal was knocked out from " +
             "under him. Without it he is dropped INSIDE whatever hit him and dies on the " +
             "very next physics step.")]
    [SerializeField] private float graceSeconds = 1f;

    [Tooltip("The player's Animator. Empty = the one on this object.")]
    [SerializeField] private Animator animator;

    [Tooltip("The player's body collider, resized while riding. Empty = the circle collider " +
             "on this object.")]
    [SerializeField] private CircleCollider2D bodyCollider;

    [Tooltip("Log every mount and every loss.")]
    [SerializeField] private bool verbose = true;

    private AnimalMount current;

    // What the player looks like and how big he is on foot. Captured once, so dismounting
    // never has to be told what to go back to.
    private RuntimeAnimatorController footLook;
    private float footRadius;
    private Vector2 footOffset;

    // The fairy, and dying. Asked before the animal is spent - see TryAbsorbHit.
    private IInvincible[] invincibilitySources;

    // When the grace window after losing an animal runs out.
    private float invincibleUntil;

    public bool IsMounted { get { return current != null; } }

    /// <summary>
    /// True for a moment after the animal was lost.
    ///
    /// The player is dropped on the exact spot where the thing that killed his animal still
    /// stands, so without this the next physics step would kill HIM - one touch of an enemy
    /// would cost the animal and a life together. A short window lets him step away, which is
    /// what the original game does.
    ///
    /// It is the same IInvincible the fairy and the death animation answer, so PlayerDeath
    /// and PlayerHurt honour it without a line of new code in either (Open/Closed).
    /// </summary>
    public bool IsInvincible { get { return Time.time < invincibleUntil; } }

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

        // Includes THIS component, and deliberately so: it is what stops a second animal,
        // picked up during the grace window, from being spent by the same enemy the player
        // is still standing in.
        invincibilitySources = GetComponents<IInvincible>();
    }

    // Subscribed in Start and dropped only in OnDestroy, NOT in the usual OnEnable/OnDisable
    // pair. This component is an InputDrivenBehaviour, so PlayerDeath switches it off for the
    // length of the death animation - and an OnDisable unsubscribe would let go of the very
    // event it is waiting for. Same reasoning as RespawnTimer.
    private void Start()
    {
        PlayerDeath.OnPlayerDied -= Dismount;
        PlayerDeath.OnPlayerDied += Dismount;
    }

    private void OnDestroy()
    {
        PlayerDeath.OnPlayerDied -= Dismount;
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
        {
            if (verbose)
                Debug.Log("[Mount] Already riding " + animal.GetType().Name);

            return;
        }

        // The old one is let go first, so its attack can never stay armed under the new one.
        if (current != null)
            ReleaseCurrent();

        current = animal;
        ApplyLook(animal.MountedLook);
        ApplyCollider(animal.ColliderRadius, animal.ColliderOffset);

        if (current.Weapon != null)
            current.Weapon.Equip();

        if (verbose)
            Debug.Log("[Mount] Riding " + animal.GetType().Name);
    }

    public void Dismount()
    {
        if (current == null)
            return;

        if (verbose)
            Debug.Log("[Mount] Lost " + current.GetType().Name);

        ReleaseCurrent();

        ApplyLook(footLook);
        ApplyCollider(footRadius, footOffset);
    }

    // ===================== IHitAbsorber =====================

    /// <summary>
    /// The animal takes the hit and is gone; the player is not touched at all - no power
    /// lost, no life lost, no knockback.
    ///
    /// The invincibility check is what stops a fairy and an animal from both being spent on
    /// one hit: if something is already protecting him the hit will be refused further down
    /// the line anyway, so the animal stays. It is asked here rather than by the hazard
    /// because only the absorber knows whether it is worth spending itself.
    /// </summary>
    public bool TryAbsorbHit(GameObject source)
    {
        if (current == null)
            return false;

        if (IsProtected())
            return false;

        Dismount();
        Smash(source);

        // Set only here, and not inside Dismount: stepping off because the game restarted or
        // because the player died is not a hit, and owes him no grace.
        invincibleUntil = Time.time + graceSeconds;

        return true;
    }

    /// <summary>
    /// Riding into an obstacle destroys it as well - the stone and the campfire from the
    /// assignment, both of which already carry a Destructible for the fairy.
    ///
    /// It asks for Destructible and NOT for IForceKillable, and that is the rule rather than
    /// an oversight: enemies answer IForceKillable too, and riding into an enemy must cost
    /// the animal WITHOUT killing the enemy. Destructible means "an obstacle in the way",
    /// which is exactly the set that gets smashed.
    /// </summary>
    private void Smash(GameObject source)
    {
        if (source == null)
            return;

        // InParent: the collider that hit us is often a child of the object that owns the
        // behaviour.
        Destructible obstacle = source.GetComponentInParent<Destructible>();

        if (obstacle != null)
            obstacle.ForceKill();
    }

    // ===================== IResettable =====================

    /// <summary>A new game starts him on his own two feet, and not half-immortal.</summary>
    public void ResetToStart()
    {
        Dismount();
        invincibleUntil = 0f;
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

    private bool IsProtected()
    {
        for (int i = 0; i < invincibilitySources.Length; i++)
        {
            if (invincibilitySources[i].IsInvincible)
                return true;
        }

        return false;
    }
}
