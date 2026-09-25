/// <summary>
/// ROLE: "The death animation is playing" - read by the animator view.
/// PATTERNS: none - a state interface.
/// SOLID: I - one property.
///
/// "Is the death animation playing?" - the pause between being killed and reappearing.
///
/// The twin of IHurtState and ICrouchState: a state the animator view draws, published
/// without naming the class that owns it (Interface Segregation, Dependency Inversion).
/// </summary>
public interface IDyingState
{
    bool IsDying { get; }
}
