using UnityEngine;

/// <summary>
/// "Picking this up makes the player untouchable for a while." Today that is the fairy.
///
/// The effect is named after what it DOES, not after what grants it. It was called
/// StarPowerUp while the star was the only source, and the fairy would then have had to
/// either copy it or carry a misleading name - the same trap that four near-identical weapon
/// power-ups fell into before EquipWeaponPowerUp replaced them. A second source (a potion, a
/// boss reward) costs a three-line pickable and no new effect.
///
/// HOW LONG it lasts is not decided here. The player's PlayerInvincible owns the number, so
/// the ten seconds the assignment asks for are typed once in the Inspector instead of being
/// repeated in every pickup that grants them (Single Responsibility).
///
/// What it does NOT do is worth saying: it never learns that a pit exists. Falling into the
/// abyss kills the player through a different route entirely - see AbyssKillOnTouch - so
/// "invincible" here honestly means "nothing that hurts him can hurt him", and the one
/// exception never has to be remembered by this class or by the effect it starts.
///
/// Finding the effect on the player is PlayerComponentPowerUp's job; this class only starts it.
/// </summary>
public class InvincibilityPowerUp : PlayerComponentPowerUp<IInvincibilityEffect>
{
    protected override void Apply(IInvincibilityEffect invincible, GameObject player)
    {
        invincible.ActivateInvincibility();
    }
}
