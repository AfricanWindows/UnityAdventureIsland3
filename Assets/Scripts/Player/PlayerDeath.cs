using System.Collections;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Dying: play the death animation where he fell, THEN put him back at the start of the
/// level and tell every IPlayerDeathHandler on the player - the lives counter, the power bar,
/// the weapon, the fairy and the animal each do their own part.
///
/// The pause is the whole reason this class grew. Before it, Kill() teleported the player
/// on the same frame, so a death animation would have played at the spawn point instead of
/// where he died - the one place it must not play.
///
/// While the animation runs he is IInvincible and IMovementLock. Neither PlayerMovement nor
/// PlayerHurt nor this class's own Kill() had to be edited to respect that: all three
/// already ask every component that answers those interfaces, so dying simply became one
/// more answer. That also means he cannot die twice, walk away mid-death, or be shoved by a
/// stone while the animation plays.
///
/// A coroutine, not a Task: this object stays enabled the whole time, so Unity's own timing
/// is the simplest thing that works, and it stops by itself when the object is disabled.
///
/// It is the single implementation of IKillable, which is how enemies, their shots, hazards
/// and the two timers kill the player without any of them knowing what respawning is.
///
/// WHERE he comes back is not decided here either: that is IPlayerSpawn (PlayerSpawn), which
/// also answers the start of a new level. Every interface left on this class is a facet of
/// one thing - dying (Single Responsibility).
/// </summary>
public class PlayerDeath : MonoBehaviour, IKillable, IForceKillable, IInvincible, IMovementLock, IDyingState
{
    [Tooltip("How long the death animation is given before the player reappears at the " +
             "start of the level. Match it to the clip.")]
    [SerializeField] private float deathAnimationSeconds = 1f;

    private IPlayerSpawn spawn;

    private IInvincible[] invincibilitySources;
    private Rigidbody2D body;
    private bool isDying;

    // Everything the player steers with: movement, both jumps, crouching, weapons. Collected
    // through their shared base class, so dying switches off "being controlled" without
    // naming a single one of them - a new controllable component is covered for free.
    private InputDrivenBehaviour[] inputComponents;

    // Everyone who has a part in "what dying costs". Found once, in Awake.
    private IPlayerDeathHandler[] deathHandlers;

    /// <summary>True while the death animation plays. The animator draws from this.</summary>
    public bool IsDying { get { return isDying; } }

    /// <summary>Nothing can kill or hurt him again mid-death.</summary>
    public bool IsInvincible { get { return isDying; } }

    /// <summary>And he cannot walk out of his own death.</summary>
    public bool BlocksMovement { get { return isDying; } }

    private void Awake()
    {
        spawn = GetComponent<IPlayerSpawn>();
        body = GetComponent<Rigidbody2D>();

        if (spawn == null)
            Debug.LogError("PlayerDeath: no IPlayerSpawn on " + gameObject.name + " - after " +
                           "dying he would stay where he fell. Add a Player Spawn component.", this);

        // Includes this component. That is deliberate: it is what makes Kill() refuse a
        // second death while the first one is still playing.
        invincibilitySources = GetComponents<IInvincible>();
        inputComponents = GetComponents<InputDrivenBehaviour>();

        // true = include inactive, so a handler on a switched-off child is still told.
        deathHandlers = GetComponentsInChildren<IPlayerDeathHandler>(true);
    }

    private void OnDisable()
    {
        // Unity kills coroutines on disable, so the routine below never reaches its
        // second half. Everything it switched off has to be switched back on here, or
        // the player returns immortal, frozen and unable to move.
        if (!isDying)
            return;

        isDying = false;
        SetControlEnabled(true);

        if (body != null)
            body.simulated = true;
    }

    /// <summary>
    /// Starts dying. The consequences - a life gone, a full power bar, the weapon lost -
    /// are announced only when the animation is over, so the Game Over popup does not land
    /// on top of it.
    /// </summary>
    public void Kill()
    {
        // ANY protection refuses a blow - the fairy, the recovery window after a hit, and
        // this death itself. ForceKill below is the one door that never asks.
        if (invincibilitySources.AnyActive())
            return;

        BeginDeath();
    }

    /// <summary>
    /// Dying with no way out - the abyss, and nothing else in the game.
    ///
    /// It is the same death, minus one question: whether anything is protecting him. The
    /// fairy and the death animation both answer through IInvincible, and this
    /// method simply never asks. That is the whole implementation of "only the pit can kill
    /// the player even with the fairy" - a rule that lives in one place instead of as a tick
    /// box on every hazard in two levels.
    ///
    /// The player is on the RECEIVING end of IForceKillable here, while the fairy is on the
    /// giving end of the same interface. One idea - "an end nothing survives" - serving both
    /// directions, which is why there is no second interface for pits.
    /// </summary>
    public void ForceKill()
    {
        BeginDeath();
    }

    /// <summary>
    /// The one road into dying, so the two entry points above cannot drift apart.
    ///
    /// The isDying guard used to come for free: Kill() asks every IInvincible, and this
    /// component reports itself invincible while the animation plays. ForceKill does not ask,
    /// so the guard has to be stated here - otherwise falling into a pit during the death
    /// animation would start a second death on top of the first.
    /// </summary>
    private void BeginDeath()
    {
        if (isDying)
            return;

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        isDying = true;
        SetControlEnabled(false);

        // Frozen where he fell: no gravity, no collisions, no sliding on. The
        // animation is the only thing that moves.
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }
        yield return new WaitForSeconds(deathAnimationSeconds);

        if (body != null)
            body.simulated = true;

        isDying = false;

        if (spawn != null)
            spawn.ReturnToSpawn();

        SetControlEnabled(true);

        for (int i = 0; i < deathHandlers.Length; i++)
            deathHandlers[i].OnPlayerDied();
    }

    /// <summary>
    /// Switches the whole control surface off and on. Disabling the components - rather
    /// than asking each of them to check a flag - means jumping and throwing really do
    /// not run, instead of running and quietly having no effect.
    /// </summary>
    private void SetControlEnabled(bool value)
    {
        for (int i = 0; i < inputComponents.Length; i++)
            inputComponents[i].enabled = value;
    }
}
