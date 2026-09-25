using Game.Weapons;

/// <summary>
/// ROLE: The axe lying in the level.
/// PATTERNS: Factory Method - inherited from WeaponPickable; Generics - one line.
///
/// The axe pickup lying in the level.
///
/// It grants the weapon itself, not ammunition: in Adventure Island the axe is unlimited
/// once found, and it is lost by dying rather than by being spent. Counting rounds moved
/// out of the game entirely - see WeaponsLostOnDeath for the other half of that rule.
/// Everything else is WeaponPickable's; this class only names which weapon.
/// </summary>
public class AxePickable : WeaponPickable<AxeWeapon>
{
}
