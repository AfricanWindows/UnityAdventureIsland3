using UnityEngine;

/// <summary>
/// VIEW. Turns what an enemy is already doing into Animator parameters, and does nothing else:
/// it never decides when to attack or jump, never fires anything, never moves anybody.
///
/// That split is the point. ShooterEnemy and HoppingEnemy stay the single owners of their
/// rules and have no Animator field. The shooter announces an attack through IAttacker, a hopper
/// stands on a GroundCheck - and this reads those abstractions, so any enemy that has them gets
/// animated by dropping this component on it, with no edit here and none there (Single
/// Responsibility, Open/Closed).
///
/// It is the same arrangement as PlayerAnimatorView, deliberately: one story for the player
/// and for every enemy.
///
/// Every parameter is OPTIONAL. An empty name means "this enemy has no such state", so a
/// controller that does not declare a parameter is never asked for it - which is what Unity
/// warns about, once per frame, forever. The fire-breathing snake fills in Attack and leaves
/// Grounded empty; the frog does the opposite. A new state later is a new name field and a new
/// source, and the enemies that do not have it leave the field empty.
/// </summary>
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class EnemyAnimatorView : MonoBehaviour
{
    [Tooltip("Trigger. Fired once each time the enemy attacks. Needs something that implements " +
             "IAttacker (ShooterEnemy). Leave empty if this enemy has no attack animation.")]
    [SerializeField] private string attackTrigger = "";

    [Tooltip("Bool. True while the enemy stands on the ground, false in the air. Needs a " +
             "GroundCheck. Leave empty if this enemy has no jump animation.")]
    [SerializeField] private string groundedParameter = "";

    private Animator animator;
    private IAttacker attacker;
    private IGroundCheck groundCheck;

    // Hashed once. Animator.SetBool("IsGrounded", ...) looks the name up by string on every
    // call, every frame; the int overload does not.
    private int attackHash;
    private int groundedHash;

    private bool hasAttack;
    private bool hasGrounded;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attacker = GetComponent<IAttacker>();
        groundCheck = GetComponent<IGroundCheck>();

        hasAttack = Enable(attackTrigger, attacker != null, "IAttacker", out attackHash);
        hasGrounded = Enable(groundedParameter, groundCheck != null, "GroundCheck", out groundedHash);
    }

    /// <summary>
    /// Subscribing here rather than in Awake, and unsubscribing in OnDisable, is what makes a
    /// beaten enemy behave: BaseEnemy switches it OFF instead of destroying it, and this stays
    /// attached to an event it can no longer answer otherwise.
    /// </summary>
    private void OnEnable()
    {
        if (!hasAttack)
            return;

        attacker.Attacked += OnAttacked;

        // A trigger that was set on the frame the enemy died is still pending when it comes
        // back, and the attack would play the instant it reappears.
        animator.ResetTrigger(attackHash);
    }

    private void OnDisable()
    {
        if (hasAttack)
            attacker.Attacked -= OnAttacked;
    }

    /// <summary>
    /// Grounded is a STATE, so it is polled every frame; an attack is a MOMENT, so it arrives as
    /// an event above. Each is read the way it actually happens.
    /// </summary>
    private void Update()
    {
        if (hasGrounded)
            animator.SetBool(groundedHash, groundCheck.IsGrounded);
    }

    private void OnAttacked()
    {
        animator.SetTrigger(attackHash);
    }

    /// <summary>
    /// One parameter switched on, or not. Written once so every parameter follows the same rule:
    /// empty name = silently off; a name with nothing on the enemy to drive it = a setup mistake,
    /// reported once, and the parameter stays off.
    /// </summary>
    private bool Enable(string parameterName, bool hasSource, string sourceName, out int hash)
    {
        hash = 0;

        if (string.IsNullOrEmpty(parameterName))
            return false;

        if (!hasSource)
        {
            Debug.LogWarning("EnemyAnimatorView: '" + parameterName + "' is set on " +
                             gameObject.name + ", but nothing on it is a " + sourceName +
                             " - that parameter stays off.", this);
            return false;
        }

        hash = Animator.StringToHash(parameterName);
        return true;
    }
}
