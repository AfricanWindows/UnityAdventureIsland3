using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// A range: everything IActivatable on this object sleeps until the player is close, and goes
/// back to sleep when he walks away.
///
/// It knows nothing about snakes, ghosts, jumping or shooting. It asks for IActivatable and
/// calls two methods, so the same component wakes a hopping snake, a fire-breathing snake and
/// a ghost written later, with no edit here (Open/Closed, Dependency Inversion). What "asleep"
/// means is each enemy's own business - see IActivatable.
///
/// WHY A DISTANCE AND NOT A TRIGGER COLLIDER
/// A trigger looks like the obvious answer and is a trap here. Unity delivers collision and
/// trigger messages to the object that owns the Rigidbody2D, so a big range collider parked on
/// a child of the enemy still reports to the enemy - and KillPlayerOnTouch, sitting there,
/// would kill the player the moment he entered the RANGE instead of when he touched the snake.
/// Escaping that needs a second Rigidbody2D on the child and a layer matrix to match: a lot of
/// setup that can be quietly got wrong in the editor, for a question that is one subtraction.
///
/// Measuring the distance instead means one component, on the enemy itself, nothing to wire,
/// and no way for the range to interfere with anything the enemy already does.
///
/// It does not search for the player: IPlayerProvider is resolved once by GameInstaller, so
/// twenty enemies do not run twenty scene-wide searches (Dependency Inversion).
/// </summary>
[DisallowMultipleComponent]
public class ActivateNearPlayer : MonoBehaviour, IInjectable
{
    [Tooltip("How close the player has to be, in units, before this object wakes up. It falls " +
             "asleep again as soon as he is further away than this.")]
    [SerializeField] private float range = 6f;

    private IPlayerProvider playerProvider;

    // Looked up once, never while measuring.
    private IActivatable[] targets;

    // What the targets were last told. The two methods are called only when the answer
    // CHANGES - an enemy that is told "wake up" sixty times a second would restart its
    // countdown sixty times a second and never actually do anything.
    //
    // It starts as TRUE because that is the truth: an IActivatable is awake until something
    // tells it otherwise. Starting it at false would make the first "go to sleep" look like
    // no change at all, the guard below would swallow it, and the enemy would carry on as if
    // this component were not there.
    private bool isAwake = true;

    /// <summary>Called by GameInstaller before Awake.</summary>
    public void Inject(IServiceResolver container)
    {
        if (container != null)
            container.TryResolve(out playerProvider);
    }

    private void Awake()
    {
        targets = GetComponents<IActivatable>();

        if (targets.Length == 0)
            Debug.LogError("ActivateNearPlayer: nothing on " + gameObject.name +
                           " implements IActivatable - there is nothing for the range to " +
                           "wake up.", this);

        if (playerProvider == null)
            Debug.LogError("[DI] ActivateNearPlayer was never injected - add a GameInstaller " +
                           "to the scene.", this);
    }

    /// <summary>
    /// The first measurement, in Start so that every enemy has finished its own Awake first.
    /// A far-away enemy is put to sleep here; a near one is simply left awake, which it
    /// already is. An enemy carrying no range therefore needs no setting to say it is awake -
    /// the sleeping is done BY this component, never configured on the enemy.
    /// </summary>
    private void Start()
    {
        Evaluate();
    }

    private void Update()
    {
        Evaluate();
    }

    private void Evaluate()
    {
        if (playerProvider == null)
            return;

        Transform player = playerProvider.PlayerTransform;

        // He can be missing for a moment during a respawn. Leave the enemy as it was rather
        // than putting it to sleep and waking it again a frame later.
        if (player == null)
            return;

        // Squared distance: the comparison is the same and it skips a square root per frame
        // per enemy.
        float sqrDistance = (player.position - transform.position).sqrMagnitude;

        SetAwake(sqrDistance <= range * range);
    }

    private void SetAwake(bool value)
    {
        if (isAwake == value)
            return;

        isAwake = value;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
                continue;

            if (value)
                targets[i].Activate();
            else
                targets[i].Deactivate();
        }
    }

    /// <summary>Draws the range in the Scene view, so it can be judged by eye.</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
