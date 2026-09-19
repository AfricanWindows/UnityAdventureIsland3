using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// An enemy that hops: stands still, jumps, lands, stands still again - the snake and the frog.
///
/// It owns the CYCLE and nothing else. The three other questions are answered by components
/// sitting next to it:
///   HOW the jump behaves (height, gravity) - JumpBehaviour, the same one the player carries
///   WHERE the hop goes                     - HopAim: a fixed hop left (snake), at the player (frog)
///   WHETHER it moves at all                - IActivatable, switched by ActivateNearPlayer
/// So the snake and the frog are this same class with a different HopAim and different
/// numbers; neither needs a line of code of its own (Strategy, Open/Closed).
///
/// It never turns round. As in the original game every enemy looks and hops LEFT - the way
/// the art is drawn - so there is no facing to track and nothing to flip.
///
/// It does not walk: the sideways speed is given once, at the push-off, and taken away on
/// landing, so it really stands still between hops.
///
/// Like every enemy here it owns only its behaviour: hurting the player on contact is
/// KillPlayerOnTouch, coming back after being beaten is RespawnTimer, and health is
/// BaseEnemy.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundCheck))]
public class HoppingEnemy : BaseEnemy, IActivatable
{
    [Tooltip("Shortest pause on the ground between hops, in seconds, counted from the moment " +
             "it LANDS - so the pause is the same whether the hop was long or short.")]
    [FormerlySerializedAs("pauseTime")]
    [SerializeField] private float minPause = 1f;

    [Tooltip("Longest pause. Every pause is picked at random between Min and Max, so the hops " +
             "cannot be timed. Set both to the same number for a steady rhythm.")]
    [SerializeField] private float maxPause = 1f;

    private Rigidbody2D rigid;
    private IGroundCheck groundCheck;
    private JumpBehaviour jump;
    private HopAim aim;

    // Awake by default, so an enemy with no range simply hops. Only ActivateNearPlayer ever
    // turns this off.
    private bool active = true;

    // Set when it lands, so the pause is a pause on the GROUND and the time spent flying
    // does not eat into it.
    private float nextHopTime;

    // Landing is "was off the ground, is on it now", and that needs the previous answer.
    private bool wasInAir;

    protected override void OnAwake()
    {
        rigid = GetComponent<Rigidbody2D>();
        groundCheck = GetComponent<IGroundCheck>();

        // Both asked for as abstract bases, so this class never names a concrete jump or aim.
        jump = GetComponent<JumpBehaviour>();
        aim = GetComponent<HopAim>();

        if (jump == null)
            Debug.LogError("HoppingEnemy: no JumpBehaviour on " + gameObject.name +
                           " - add a Variable Height Jump component.", this);

        if (aim == null)
            Debug.LogError("HoppingEnemy: no HopAim on " + gameObject.name +
                           " - add a Forward Hop Aim or a Player Hop Aim component.", this);
    }

    /// <summary>
    /// Also runs when a beaten enemy is revived, so it always stands for a moment before
    /// hopping again instead of leaping the instant it comes back.
    /// </summary>
    private void OnEnable()
    {
        nextHopTime = Time.time + NextPause();
        wasInAir = false;
    }

    /// <summary>The player came into range. Stand for one pause, then carry on hopping.</summary>
    public void Activate()
    {
        active = true;
        nextHopTime = Time.time + NextPause();
    }

    /// <summary>
    /// The player left. It stops starting NEW hops; the one it is in finishes normally,
    /// because freezing an enemy in mid-air looks like a bug.
    /// </summary>
    public void Deactivate()
    {
        active = false;
    }

    private void FixedUpdate()
    {
        if (jump == null || aim == null || rigid == null || groundCheck == null)
            return;

        if (groundCheck.IsInAir())
        {
            wasInAir = true;
            jump.ApplyAirPhysics(Time.fixedDeltaTime);
            return;
        }

        // Still going up while the collision is still reported: we pushed off a moment ago and
        // physics has not caught up yet. Braking here would kill the hop before it began.
        if (jump.IsRising)
            return;

        if (wasInAir)
        {
            wasInAir = false;
            nextHopTime = Time.time + NextPause();
        }

        StandStill();

        // Asleep: it still stands properly, it just never starts the next hop.
        if (!active)
            return;

        if (Time.time >= nextHopTime)
            Hop();
    }

    /// <summary>A fresh random pause between Min and Max. A Max below Min counts as Min.</summary>
    private float NextPause()
    {
        return Random.Range(minPause, Mathf.Max(minPause, maxPause));
    }

    /// <summary>Kills the speed left over from the last hop, so it really stands.</summary>
    private void StandStill()
    {
        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }

    private void Hop()
    {
        // Sideways first, then up. The aim is asked NOW, at the push-off - for the frog this is
        // the moment it looks at where the player is standing.
        rigid.linearVelocity = new Vector2(aim.GetHorizontalSpeed(jump), rigid.linearVelocity.y);
        jump.Begin();
    }
}
