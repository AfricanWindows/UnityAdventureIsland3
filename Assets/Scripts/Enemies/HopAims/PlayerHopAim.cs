using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// Lands where the player was standing at the moment of the push-off - the frog.
///
/// The spot is taken ONCE, as the frog leaves the ground, and never updated in the air. That
/// is what makes it fair: the player sees the frog coming and can step out of the way.
///
/// The arc depends only on the DISTANCE to that spot, never on where in the level the frog or
/// the player happen to be: from x = 5 to x = 1 is the same hop as from x = 1 to x = -3. The
/// height comes from the JumpBehaviour; this works out the one number left - how fast to move
/// sideways so the frog comes down on the spot: distance / time in the air. A steady sideways
/// speed plus gravity is exactly what draws the parabola.
///
/// It does not search for the player: IPlayerProvider arrives from GameInstaller.
/// </summary>
public class PlayerHopAim : HopAim, IInjectable
{
    [Tooltip("The farthest one hop may carry, in units. Without it a far-away player would " +
             "send the frog across half the level in a single jump.")]
    [Min(0f)]
    [SerializeField] private float maxDistance = 6f;

    private IPlayerProvider playerProvider;

    /// <summary>Called by GameInstaller before Awake.</summary>
    public void Inject(IServiceContainer container)
    {
        if (container != null)
            container.TryResolve(out playerProvider);
    }

    private void Awake()
    {
        if (playerProvider == null)
            Debug.LogError("[DI] PlayerHopAim was never injected - add a GameInstaller to the scene.", this);
    }

    public override float GetHorizontalSpeed(JumpBehaviour jump)
    {
        Transform player = playerProvider != null ? playerProvider.PlayerTransform : null;

        // Nobody to aim at: hop straight up and down on the spot.
        if (player == null || jump == null)
            return 0f;

        // The snapshot. Read once, here, at the push-off - never again during the flight.
        Vector2 toPlayer = player.position - transform.position;

        float distance = Mathf.Clamp(toPlayer.x, -maxDistance, maxDistance);
        float airTime = jump.GetAirTime(toPlayer.y);

        return airTime > 0f ? distance / airTime : 0f;
    }

    /// <summary>
    /// At the player, so the frog watches him while it waits - and is already facing the way
    /// it jumps when it takes off.
    /// </summary>
    public override float GetFacing()
    {
        Transform player = playerProvider != null ? playerProvider.PlayerTransform : null;

        if (player == null)
            return 0f;

        return transform.HorizontalDirectionTo(player.position);
    }
}
