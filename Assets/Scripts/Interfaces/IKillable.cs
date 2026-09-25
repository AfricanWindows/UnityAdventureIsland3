/// <summary>
/// ROLE: "Kill me by a blow" (an enemy's touch, a shot, the campfire) - protections may refuse.
/// PATTERNS: none - a role interface.
/// SOLID: I - one method.
///
/// Something that can be killed by a BLOW - an enemy's touch, its shot, the campfire - as
/// opposed to being damaged. One method, so a caller that only needs to kill is not also
/// handed respawning, invincibility or the death event (Interface Segregation).
///
/// A blow may be refused: the fairy and the recovery window after a hit protect against it.
/// What must never be refused - the abyss and the empty power bar - goes through
/// IForceKillable instead.
///
/// KillPlayerOnTouch and EnemyProjectile depend on THIS, not on PlayerDeath, so neither of
/// them ever has to know how dying is implemented (Dependency Inversion).
/// </summary>
public interface IKillable
{
    void Kill();
}
