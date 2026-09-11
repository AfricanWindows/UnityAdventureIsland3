using System;
using System.Collections;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Dying: play the death animation where he fell, THEN put him back at the start of the
/// level and tell the rest of the game - the lives counter, the power bar and the weapons
/// all listen.
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
/// </summary>
public class PlayerDeath : MonoBehaviour, IKillable, ILevelStartHandler, IInvincible, IMovementLock
{
    [Tooltip("How long the death animation is given before the player reappears at the " +
             "start of the level. Match it to the clip.")]
    [SerializeField] private float deathAnimationSeconds = 1f;

    public static event Action OnPlayerDied;

    private Vector3 startPositon;

    private IInvincible[] invincibilitySources;
    private Rigidbody2D body;
    private bool isDying;

    // Everything the player steers with: movement, both jumps, crouching, weapons. Collected
    // through their shared base class, so dying switches off "being controlled" without
    // naming a single one of them - a new controllable component is covered for free.
    private InputDrivenBehaviour[] inputComponents;

    /// <summary>True while the death animation plays. The animator draws from this.</summary>
    public bool IsDying { get { return isDying; } }

    /// <summary>Nothing can kill or hurt him again mid-death.</summary>
    public bool IsInvincible { get { return isDying; } }

    /// <summary>And he cannot walk out of his own death.</summary>
    public bool BlocksMovement { get { return isDying; } }

    private void Awake()
    {
        startPositon = transform.position;
        body = GetComponent<Rigidbody2D>();

        // Includes this component. That is deliberate: it is what makes Kill() refuse a
        // second death while the first one is still playing.
        invincibilitySources = GetComponents<IInvincible>();
        inputComponents = GetComponents<InputDrivenBehaviour>();
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
    /// A level began: this is where the player now stands, and where dying will bring
    /// him back - level two sends him to level two, not to where the game started.
    /// </summary>
    public void OnLevelStarted(Vector3 spawnPosition)
    {
        startPositon = spawnPosition;
        Respawn();
    }

    /// <summary>
    /// Back to the start of the current level.
    ///
    /// The velocity is wiped too. Without it a player who died while falling arrives at the
    /// spawn point still falling at the speed that killed him, and drops straight through
    /// the floor on the frame he reappears.
    /// </summary>
    public void Respawn()
    {
        transform.position = startPositon;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Starts dying. The consequences - a life gone, a full power bar, the weapon lost -
    /// are announced only when the animation is over, so the Game Over popup does not land
    /// on top of it.
    /// </summary>
    public void Kill()
    {
        if (IsAnySourceInvincible())
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

        Respawn();

        SetControlEnabled(true);

        if (OnPlayerDied != null)
            OnPlayerDied();
    }

    /// <summary>
    /// True while ANY invincibility source is active - the star, this death, and the fairy
    /// tomorrow, with no change needed here.
    /// </summary>
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

    private bool IsAnySourceInvincible()
    {
        for (int i = 0; i < invincibilitySources.Length; i++)
        {
            if (invincibilitySources[i].IsInvincible)
                return true;
        }

        return false;
    }
}
