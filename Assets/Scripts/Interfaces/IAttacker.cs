using System;

/// <summary>
/// Something that attacks, and says so the moment it does.
///
/// It exists so that showing the attack - a sprite, a sound, a puff of smoke - never has to
/// be written inside the thing that decides WHEN to attack. EnemyAnimatorView listens to it on
/// the fire-breathing snake, PlayerAnimatorView on every weapon and animal attack the player
/// carries - and neither view needs an edit for an attacker written later (Open/Closed,
/// Dependency Inversion).
///
/// An event rather than a "IsAttacking" property on purpose: an attack is a MOMENT, not a
/// state. A listener that has to poll for it would need the attacker to keep a flag raised
/// for exactly as long as the listener happens to need it, which is the listener's business.
/// </summary>
public interface IAttacker
{
    /// <summary>Raised on the frame an attack actually happens.</summary>
    event Action Attacked;
}
