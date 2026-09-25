/// <summary>
/// ROLE: "Something is shoving me right now" - read by the animator view.
/// PATTERNS: none - a state interface.
/// SOLID: I - one property.
///
/// "Is something shoving me right now?" - the moment after a hazard hurt the player.
///
/// One property, for the same reason ICrouchState and IHidingState exist: the animator view
/// needs to DRAW a state, and drawing it must not require knowing the class that owns it
/// (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IHurtState
{
    bool IsHurt { get; }
}
