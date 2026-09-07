/// <summary>
/// "Right now the player must not walk."
///
/// Lying down is one reason. Being stunned, sliding, opening a door, standing in a
/// cut-scene are others, and every one of them is a new component that implements this -
/// PlayerMovement is never edited again (Open/Closed).
///
/// This mirrors how PlayerDeath already asks every IInvincible on the object whether the
/// player can be hurt: one question, many independent answerers, and the asker never
/// learns what a star or a fairy is (Dependency Inversion).
///
/// It locks WALKING only. Jumping, aiming and facing are separate questions, so a lock
/// that should also stop those says so through its own interface (Interface Segregation).
/// </summary>
public interface IMovementLock
{
    /// <summary>True while this source is holding the player still.</summary>
    bool BlocksMovement { get; }
}
