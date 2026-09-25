/// <summary>
/// Something that can be taken out of the game - an enemy beaten, a fruit eaten, an egg
/// opened - and later brought back.
///
/// The "taken out" half, the Defeated event, is IDefeatable: a listener that only wants the
/// news (DropOnDefeat) depends on that alone. This adds the "brought back" half. Together
/// they are all a timer that respawns things needs, so it never learns what an enemy, a
/// fruit or an egg is, what health is, or how one dies (Dependency Inversion, Interface
/// Segregation). That is why the one RespawnTimer serves all of them.
/// </summary>
public interface IRespawnable : IDefeatable
{
    /// <summary>True while it is out of the game.</summary>
    bool IsDefeated { get; }

    /// <summary>Bring it back.</summary>
    void Revive();
}
