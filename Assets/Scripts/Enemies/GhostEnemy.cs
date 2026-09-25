using UnityEngine;

/// <summary>
/// ROLE: The ghost: chases the player, freezes and hides while he looks at it (the Boo rule).
/// PATTERNS: Template Method - overrides the CanMove hook of ChasingEnemy.
/// SOLID: O - one rule added, the chase untouched.
///
/// The ghost: a chaser that is shy. While the player is in range it comes straight at him -
/// but the moment he turns to face it, it freezes and hides its face, and it only moves again
/// once he looks away.
///
/// Everything else it inherits or borrows. The chase is ChasingEnemy, and this class adds
/// exactly ONE rule to it through the CanMove hook (Template Method, Open/Closed). "Nothing
/// but the fairy can destroy it" is not written here at all: it is Weapon Proof on BaseEnemy,
/// ticked on the prefab, and the fairy gets through because her touch is ForceKill, which no
/// armour stops.
///
/// "Hiding" is not stored anywhere. It is worked out from the player's facing every time it
/// is asked, so there is no flag to clear on a respawn or a restart, and the pose on screen
/// can never disagree with the rule that stops the ghost.
/// </summary>
public class GhostEnemy : ChasingEnemy, IHidingState
{
    // The player's IFacing, fetched once per player object instead of once per step.
    private GameObject facingOwner;
    private IFacing playerFacing;

    /// <summary>
    /// Read by EnemyAnimatorView: true = the GhostHide frame, false = GhostChase. Only while
    /// the player is in range - out of range nobody is chasing or hiding from anybody.
    /// </summary>
    public bool IsHiding
    {
        get { return IsActive && IsWatched(); }
    }

    /// <summary>The one rule the ghost adds: it never moves while it is being looked at.</summary>
    protected override bool CanMove()
    {
        return !IsWatched();
    }

    /// <summary>Is the player facing this ghost right now?</summary>
    private bool IsWatched()
    {
        Transform player = Player;

        if (player == null)
            return false;

        IFacing looks = PlayerFacing(player.gameObject);

        if (looks == null)
            return false;

        float towardsGhost = player.HorizontalDirectionTo(transform.position);

        // Straight above or below him: he is looking neither at it nor away from it, and a
        // ghost that froze there would hang over his head for ever. It keeps coming.
        if (towardsGhost == 0f)
            return false;

        return Mathf.Sign(looks.FacingDirection) == towardsGhost;
    }

    private IFacing PlayerFacing(GameObject player)
    {
        if (player != facingOwner)
        {
            facingOwner = player;
            playerFacing = player.GetComponent<IFacing>();

            if (playerFacing == null)
                Debug.LogError("GhostEnemy: the player has no IFacing - the ghost cannot " +
                               "tell whether he is looking at it.", this);
        }

        return playerFacing;
    }
}
