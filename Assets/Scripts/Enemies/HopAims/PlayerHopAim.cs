using Game.Core;
using Game.Core.DI;
using UnityEngine;

/// <summary>
/// ROLE: Hop aim of the frog: lands where the player stood at the moment of take-off.
/// PATTERNS: Strategy - a concrete HopAim; DI - gets IPlayerProvider in Inject.
///
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
/// LEFT ONLY, like every enemy in the original. A player who has already got past the frog
/// stands to its right; the aim then answers 0 and the frog hops straight up on the spot.
/// That is one clamp, not a second behaviour - no "idle" state, no turning round.
///
/// It does not search for the player: IPlayerProvider arrives from GameInstaller.
/// </summary>
public class PlayerHopAim : HopAim, IInjectable
{
    [Tooltip("The farthest one hop may carry to the left, in units. Without it a far-away " +
             "player would send the frog across half the level in a single jump.")]
    [Min(0f)]
    [SerializeField] private float maxDistance = 6f;

    private IPlayerProvider playerProvider;

    /// <summary>Called by GameInstaller before Awake.</summary>
    public void Inject(IServiceResolver container)
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

        // Left only: a player to the RIGHT (already past the frog) clamps to 0 - a hop on
        // the spot.
        float distance = Mathf.Clamp(toPlayer.x, -maxDistance, 0f);
        float airTime = jump.GetAirTime(toPlayer.y);

        return airTime > 0f ? distance / airTime : 0f;
    }
}
