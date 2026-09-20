using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// An enemy that flies straight at the player, through anything in the way - the ghost.
///
/// It is named for what it DOES, not for the ghost, because nothing here is ghostly: a bee or
/// a bat that homes in on the player is this same class with a different sprite. What makes
/// the ghost a ghost is added on top by GhostEnemy (freezing while watched) and by the prefab
/// (Weapon Proof on BaseEnemy).
///
/// Straight at him, re-aimed every physics step: when the player moves, the next step already
/// points at where he is now. No path, no memory of where he was.
///
/// Like every enemy here it owns only its movement. Waking up near the player is
/// ActivateNearPlayer, killing on contact is KillPlayerOnTouch, coming back after the fairy is
/// RespawnTimer, health is BaseEnemy (Single Responsibility).
///
/// THE BODY MUST BE KINEMATIC, AND THE COLLIDER A TRIGGER. Kinematic, so nothing pushes it off
/// course and gravity does not pull it down; a trigger, because a kinematic body with a solid
/// collider SHOVES dynamic bodies aside - it would push the player and knock the snakes out of
/// their hops. A trigger passes through walls and enemies and still reports every touch.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ChasingEnemy : ActivatableEnemy, IInjectable, IFacing
{
    [Tooltip("Units per second. Keep it below the player's walking speed (5) so he can get " +
             "away - once he is further than the Activate Near Player range, it stops.")]
    [Min(0f)]
    [SerializeField] private float speed = 2f;

    private IPlayerProvider playerProvider;
    private Rigidbody2D body;

    // Left by default, which is how the art is drawn.
    private float facing = -1f;

    /// <summary>Read by FacingView: it looks at the player it is chasing.</summary>
    public float FacingDirection { get { return facing; } }

    /// <summary>For subclasses: the player right now, or null during a respawn.</summary>
    protected Transform Player
    {
        get { return playerProvider != null ? playerProvider.PlayerTransform : null; }
    }

    /// <summary>Called by GameInstaller before Awake.</summary>
    public void Inject(IServiceResolver container)
    {
        if (container != null)
            container.TryResolve(out playerProvider);
    }

    protected override void OnAwake()
    {
        body = GetComponent<Rigidbody2D>();

        if (playerProvider == null)
            Debug.LogError("[DI] ChasingEnemy was never injected - add a GameInstaller to the scene.", this);
    }

    // Asleep (the player got away) it stays exactly where it is.
    private void FixedUpdate()
    {
        Transform player = Player;

        if (!IsActive || player == null || body == null)
            return;

        // Turned towards him even when it is not allowed to move - a frozen ghost still looks
        // at the player who froze it.
        float direction = transform.HorizontalDirectionTo(player.position);
        if (direction != 0f)
            facing = direction;

        if (!CanMove())
            return;

        // MoveTowards never overshoots: the last step stops exactly on the target, so it does
        // not jitter back and forth around the player.
        Vector2 next = Vector2.MoveTowards(body.position, player.position, speed * Time.fixedDeltaTime);
        body.MovePosition(next);
    }

    /// <summary>
    /// TEMPLATE METHOD hook: may it move this step? A plain chaser always may. The ghost says
    /// no while the player is looking at it - and the chase itself stays written only here, so
    /// a subclass can add a rule but never break the movement (Open/Closed).
    /// </summary>
    protected virtual bool CanMove()
    {
        return true;
    }
}
