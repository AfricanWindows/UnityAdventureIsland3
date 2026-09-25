using UnityEngine;

/// <summary>
/// ROLE: One effect a pickup gives the player.
/// PATTERNS: Composite - the component interface; Factory Method - what CreatePowerUp returns.
/// SOLID: O - a new effect is a new class.
///
/// One effect a pickup gives the player - refill the bar, count a fruit, start the fairy, put
/// a weapon in his hand, sit him on an animal.
///
/// It is the PRODUCT of the pickables' Factory Method: each BasePickable creates one in
/// CreatePowerUp() and hands it over through IPowerUpCollector, never learning what it does.
/// A plain C# object, so the same effect can be granted by a fruit, an egg or anything else
/// without being copied (Single Responsibility, Open/Closed).
/// </summary>
public interface IPowerUp
{
    void ApplyPowerUp(GameObject player);
}
