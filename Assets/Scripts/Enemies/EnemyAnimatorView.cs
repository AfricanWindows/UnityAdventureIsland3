using UnityEngine;

/// <summary>
/// VIEW. Turns what an enemy is already doing into Animator parameters, and does nothing else:
/// it never decides when to attack, never fires anything, never moves anybody.
///
/// That split is the point. ShooterEnemy stays the single owner of the shooting rules and has
/// no Animator field; it announces an attack through IAttacker and does not care who listens.
/// Any enemy that raises the same event gets its mouth animated by dropping this component on
/// it, with no edit here and none there (Single Responsibility, Open/Closed).
///
/// It is the same arrangement as PlayerAnimatorView, deliberately: one story for the player
/// and for every enemy.
///
/// An empty parameter name means "this enemy has no such state", so a controller that does not
/// declare a parameter is never asked for it - which is what Unity warns about, once per
/// frame, forever. That is also how this component grows later: a new state is a new name
/// field and a new listener, and the enemies that do not have it leave the field empty.
/// </summary>
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class EnemyAnimatorView : MonoBehaviour
{
    [Tooltip("Trigger. Fired once each time the enemy attacks. Leave empty if this enemy's " +
             "controller has no attack state.")]
    [SerializeField] private string attackTrigger = "Attack";

    private Animator animator;
    private IAttacker attacker;

    // Hashed once. Animator.SetTrigger("Attack") looks the name up by string on every call;
    // the int overload does not.
    private int attackHash;
    private bool hasAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attacker = GetComponent<IAttacker>();

        hasAttack = !string.IsNullOrEmpty(attackTrigger) && attacker != null;

        if (hasAttack)
            attackHash = Animator.StringToHash(attackTrigger);

        if (attacker == null)
            Debug.LogError("EnemyAnimatorView: nothing on " + gameObject.name +
                           " implements IAttacker - there is no attack to show.", this);
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

    private void OnAttacked()
    {
        animator.SetTrigger(attackHash);
    }
}
