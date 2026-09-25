/// <summary>
/// ROLE: The fairy pickup: ten seconds of invincibility.
/// PATTERNS: Factory Method - creates an InvincibilityPowerUp.
///
/// The fairy: ten seconds during which nothing in the game can touch the player.
///
/// Three lines, and that is the point - being picked up is written once in BasePickable,
/// and becoming invincible is written once in InvincibilityPowerUp. This class only names
/// which of the two goes together, exactly like AxePickable and BoomerangPickable do for
/// weapons (Single Responsibility, Don't Repeat Yourself).
///
/// The fairy is meant to come out of an egg, and it needs nothing at all for that: a
/// PickableDropper takes any BasePickable, so dragging this prefab into an egg's Random Pool
/// is the whole integration (Open/Closed).
/// </summary>
public class FairyPickable : BasePickable
{
    protected override IPowerUp CreatePowerUp()
    {
        return new InvincibilityPowerUp();
    }
}
