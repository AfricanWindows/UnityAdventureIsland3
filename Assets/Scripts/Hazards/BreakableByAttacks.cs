using UnityEngine;

/// <summary>
/// ROLE: Lets weapons and animal attacks break the stone (the damage must reach a threshold).
/// PATTERNS: none - a small component that implements IDamageable.
/// SOLID: O - the rule is which components the prefab carries, not a flag.
///
/// "Weapons and animal attacks can break this" - the stone, which the assignment says goes
/// down to a boomerang or an animal's fire.
///
/// It is a separate component from Destructible on purpose, and the pair of them is what
/// expresses the assignment's two different rules without a single flag:
///
///   Destructible        -> can be wiped out by something unstoppable: the fairy's touch,
///                          or riding into it on an animal.
///   BreakableByAttacks  -> can ALSO be broken by an ordinary attack.
///
/// The campfire carries only the first, so it survives every shot in the game and still
/// disappears when the fairy brushes it. The stone carries both. Nothing has to be told
/// which is which - the components on the prefab ARE the rule (Open/Closed).
///
/// It holds no health, but it does hold HARDNESS - the smallest blow that has any effect on
/// it at all. That one number is how the assignment's rule "the stone goes down to the
/// boomerang or an animal's fire, but not to the axe" is expressed without this class ever
/// hearing of an axe.
///
/// A threshold rather than a list of weapons that are allowed, because a list would have to
/// be kept in step with every weapon ever added, in every level, on every stone - and the
/// first weapon somebody forgot to add to it would silently do nothing. Damage is a number
/// each weapon already carries in its own config asset, so the rule becomes "a light blow
/// bounces off a stone", which is a thing a player can understand from playing rather than a
/// table only the author knows (Open/Closed).
///
/// Anything that reaches the threshold destroys it outright. An obstacle that should take
/// several real hits is a different component, or BaseEnemy, which already counts them.
///
/// It breaks itself through IForceKillable rather than deactivating the object directly, so
/// an obstacle keeps ONE way of disappearing however it was destroyed - and that one way
/// already knows how to come back on a restart (Single Responsibility). It depends on
/// IForceKillable only - no RequireComponent on the concrete Destructible - and a missing
/// one is reported in Awake (Dependency Inversion).
/// </summary>
[DisallowMultipleComponent]
public class BreakableByAttacks : MonoBehaviour, IDamageable
{
    [Tooltip("The weakest blow that breaks this. Anything below it bounces off with no " +
             "effect. The stone is 2: the axe deals 1 and cannot touch it, the boomerang " +
             "and the animals' attacks deal 2 and destroy it.")]
    [Min(1)]
    [SerializeField] private int minimumDamage = 2;

    private IForceKillable self;

    private void Awake()
    {
        self = GetComponent<IForceKillable>();

        if (self == null)
            Debug.LogError("BreakableByAttacks: nothing on " + gameObject.name +
                           " can be destroyed - add a Destructible.", this);
    }

    public bool TakeDamage(int amount)
    {
        if (self == null)
            return false;

        // Too light to matter - the axe against a stone. Said nothing about, because from
        // the player's side this is a projectile bouncing off, not an error.
        if (amount < minimumDamage)
            return false;

        self.ForceKill();
        return true;
    }
}
