/// <summary>
/// "Right now the attack button belongs to ME, and this is what it fires."
///
/// Riding an animal is the one reason today: while the player is mounted the button throws
/// the animal's attack, and the axe simply waits in the slot until he is back on foot.
///
/// It REPLACED IAttackLock, and the difference is the whole point. The lock only said "do
/// not fire", so the animal needed a trigger finger of its own - two components reading the
/// same button on the same frame, kept apart by a flag they both had to respect. Now there
/// is ONE reader of the button (WeaponsHandler), and a mount answers the only question that
/// was ever really being asked: which weapon is the button wired to right now?
///
/// WeaponsHandler still never learns what an animal is - it asks this interface and takes
/// whatever comes back (Open/Closed, Dependency Inversion). A stun that swaps in a "jammed"
/// weapon, or a boss phase with a scripted attack, is a new component and no edit there.
/// </summary>
public interface IAttackOverride
{
    /// <summary>
    /// The weapon that has taken over the button, or null when this source is not claiming
    /// it. Null is the normal answer - the player is on foot almost all of the time.
    /// </summary>
    IUseableWeapon OverrideWeapon { get; }
}
