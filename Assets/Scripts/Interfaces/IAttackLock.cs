/// <summary>
/// "Right now the weapon in his hand must not fire."
///
/// Riding an animal is the one reason today: the same button attacks with the animal
/// instead, and the axe simply waits in the slot until the player is back on foot.
///
/// It is the exact twin of IMovementLock, and for the same reason: WeaponsHandler asks one
/// question, any number of independent components may answer it, and it never learns what
/// an animal is (Open/Closed, Dependency Inversion). A stun, a cut-scene or a boss intro
/// that must stop shooting is a new component and no edit here.
///
/// It blocks the WEAPON, not attacking in general - the mount fires its own attack while
/// this says true, which is precisely the point (Interface Segregation).
/// </summary>
public interface IAttackLock
{
    /// <summary>True while this source is holding the trigger shut.</summary>
    bool BlocksAttack { get; }
}
