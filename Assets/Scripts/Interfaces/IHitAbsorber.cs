using UnityEngine;

/// <summary>
/// "Something is about to hurt the player - can you take it instead of him?"
///
/// The animal is the answer today: riding one, any hit costs the animal and nothing else.
/// The player keeps his power, his lives and his weapon, and simply lands back on his feet.
///
/// It is NOT invincibility, and that is why it is a separate interface. IInvincible is a
/// question - "may he be hurt at all" - answered the same way however many times it is
/// asked. This is a TRANSACTION: answering yes SPENDS something, so it may only be asked
/// when a hit is really landing, and only once per hit (Interface Segregation).
///
/// The source is handed over because an absorber usually has something to do with whatever
/// hit it: riding into a stone destroys the stone as well as costing the animal, while
/// riding into an enemy only costs the animal. The absorber decides which - the hazard
/// never learns that animals exist.
/// </summary>
public interface IHitAbsorber
{
    /// <summary>
    /// Take this hit instead of the player.
    /// </summary>
    /// <param name="source">The object that is hurting him - a stone, a campfire, an
    /// enemy, a shot. May be null when nothing in the world is to blame.</param>
    /// <returns>True if the hit was taken and the player must be left alone.</returns>
    bool TryAbsorbHit(GameObject source);
}
