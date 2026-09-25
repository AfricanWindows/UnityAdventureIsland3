using UnityEngine;

/// <summary>
/// ROLE: View - turns what the player does into Animator parameters.
/// PATTERNS: MVC-style View; Observer - listens to IAttacker.Attacked.
/// SOLID: S - movement keeps no Animator field; D - reads interfaces only.
///
/// VIEW. Turns what the player is already doing into Animator parameters, and does
/// nothing else: it never moves him, never reads input, never decides when he may run.
///
/// That split is the whole point. PlayerMovement stays the single owner of the movement
/// rules and does not gain an Animator field; adding, removing or reworking the animation
/// touches only this file (Single Responsibility). A character with no Animator simply
/// does not carry this component.
///
/// It reads everything through small interfaces - IMovementSpeed, IGroundCheck, ICrouchState,
/// IHurtState, IDyingState, IAttacker - so it never names the class that owns them, exactly
/// like EnemyAnimatorView (Dependency Inversion).
///
/// It reads IMovementSpeed.OwnSpeedX rather than the Rigidbody, so the run cycle follows the
/// steps he takes and not anything else that moves his body.
/// </summary>
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class PlayerAnimatorView : MonoBehaviour
{
    [Header("Animator parameters")]
    [Tooltip("Float. Absolute walking speed, 0 while standing. This is the one the " +
             "Idle/Run transitions read.")]
    [SerializeField] private string speedParameter = "Speed";

    [Tooltip("Optional Bool, empty by default. Fill this in only if a jump or fall state " +
             "is ever added - with just Idle and Run there is nothing for it to switch.")]
    [SerializeField] private string groundedParameter = "";

    [Tooltip("Bool. True while the player is lying down. Leave empty if there is no lying " +
             "animation yet.")]
    [SerializeField] private string crouchParameter = "IsCrouching";

    [Tooltip("Bool. True for the moment a hazard shoves the player. Leave empty if there " +
             "is no hurt animation.")]
    [SerializeField] private string hurtParameter = "IsHurt";

    [Tooltip("Bool. True while the death animation plays. Leave empty if there is no " +
             "death animation.")]
    [SerializeField] private string deathParameter = "IsDead";

    [Tooltip("Trigger. Fired once each time a shot really leaves - a weapon or the animal " +
             "he is riding, whichever answered the button. Leave empty if there is no " +
             "attack animation.")]
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Tuning")]
    [Tooltip("Below this speed he counts as standing. Stops the run cycle from flickering " +
             "on during the last fraction of the braking ramp.")]
    [SerializeField] private float runThreshold = 0.05f;

    private Animator animator;
    private IMovementSpeed movement;
    private IGroundCheck groundCheck;
    private ICrouchState crouch;
    private IHurtState hurt;
    private IDyingState death;

    // Hashed once. Animator.SetFloat("Speed", ...) looks the name up by string on every
    // call, every frame; the int overload does not.
    private int speedHash;
    private int groundedHash;
    private int crouchHash;
    private int hurtHash;
    private int deathHash;
    private int attackHash;

    // EVERY attacker on the player, not one: the axe, the boomerang and each animal's
    // attack are separate components, and any of them may be the one that fires. Asked for
    // as IAttacker, so this view never learns which weapons or animals exist - a new one is
    // animated by existing (Open/Closed, Dependency Inversion).
    private IAttacker[] attackers;

    private bool hasSpeed;
    private bool hasGrounded;
    private bool hasCrouch;
    private bool hasHurt;
    private bool hasDeath;
    private bool hasAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<IMovementSpeed>();
        groundCheck = GetComponent<IGroundCheck>();
        crouch = GetComponent<ICrouchState>();
        hurt = GetComponent<IHurtState>();
        death = GetComponent<IDyingState>();

        // InChildren, and including inactive: the weapons live on child objects of the
        // player and start switched off until he finds them.
        attackers = GetComponentsInChildren<IAttacker>(true);

        // An empty name means "this character has no such state", so an Idle/Run-only
        // controller never gets asked for a parameter it does not declare - which is what
        // Unity warns about, once per frame, forever.
        hasSpeed = !string.IsNullOrEmpty(speedParameter);
        hasGrounded = !string.IsNullOrEmpty(groundedParameter) && groundCheck != null;
        hasCrouch = !string.IsNullOrEmpty(crouchParameter) && crouch != null;
        hasHurt = !string.IsNullOrEmpty(hurtParameter) && hurt != null;
        hasDeath = !string.IsNullOrEmpty(deathParameter) && death != null;
        hasAttack = !string.IsNullOrEmpty(attackTrigger) && attackers.Length > 0;

        if (hasSpeed)
            speedHash = Animator.StringToHash(speedParameter);

        if (hasGrounded)
            groundedHash = Animator.StringToHash(groundedParameter);

        if (hasCrouch)
            crouchHash = Animator.StringToHash(crouchParameter);

        if (hasHurt)
            hurtHash = Animator.StringToHash(hurtParameter);

        if (hasDeath)
            deathHash = Animator.StringToHash(deathParameter);

        if (hasAttack)
            attackHash = Animator.StringToHash(attackTrigger);

        if (movement == null)
            Debug.LogError("PlayerAnimatorView: no IMovementSpeed on " + gameObject.name, this);
    }

    /// <summary>
    /// Every attacker on the player is listened to at once. Only one of them can be
    /// equipped at a time, and an unequipped weapon never raises the event, so there is
    /// nothing to switch between - the animation simply follows whoever really fired.
    /// </summary>
    private void OnEnable()
    {
        if (!hasAttack)
            return;

        for (int i = 0; i < attackers.Length; i++)
            attackers[i].Attacked += OnAttacked;

        // A trigger left pending from before - a shot on the frame the player died - would
        // otherwise play the attack the moment he comes back.
        animator.ResetTrigger(attackHash);
    }

    private void OnDisable()
    {
        if (!hasAttack)
            return;

        for (int i = 0; i < attackers.Length; i++)
            attackers[i].Attacked -= OnAttacked;
    }

    /// <summary>
    /// An attack is a MOMENT, so it arrives as an event and sets a trigger; everything in
    /// Update below is a STATE, so it is polled. Each is read the way it actually happens.
    /// </summary>
    private void OnAttacked()
    {
        animator.SetTrigger(attackHash);
    }

    private void Update()
    {
        if (movement == null)
            return;

        if (hasSpeed)
        {
            float speed = Mathf.Abs(movement.OwnSpeedX);
            animator.SetFloat(speedHash, speed < runThreshold ? 0f : speed);
        }

        if (hasGrounded)
            animator.SetBool(groundedHash, groundCheck.IsGrounded);

        if (hasCrouch)
            animator.SetBool(crouchHash, crouch.IsCrouching);

        if (hasHurt)
            animator.SetBool(hurtHash, hurt.IsHurt);

        if (hasDeath)
            animator.SetBool(deathHash, death.IsDying);
    }
}
