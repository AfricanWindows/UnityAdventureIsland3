using Game.Core;
using UnityEngine;

/// <summary>
/// The short recovery window after the player takes a hit: for a moment nothing can hurt him.
///
/// Two things start it - a stone hurting him (PlayerHurt) and his animal being knocked out
/// from under him (MountHitAbsorber). Both call Begin(); neither knows how long it lasts or how
/// it is shown. How it is shown is HitInvincibilityView's job (Single Responsibility).
/// All three know it only as IHitRecovery, never as this class (Dependency Inversion).
///
/// It answers the same IInvincible as the fairy and the death animation, so PlayerDeath,
/// PlayerHurt and MountHitAbsorber honour it without a line of new code (Open/Closed). It stops
/// only what goes through Kill and TryHurt - blows. The abyss and the empty power bar use
/// ForceKill, which no protection can refuse, so a recovery window can never swallow a life.
///
/// A deadline and not a coroutine or a Task: "is Time.time still before the end?" is all
/// there is to it. Nothing is started, so nothing can be left running, and it stops by
/// itself while the game is paused because Time.time stops too.
/// </summary>
[DisallowMultipleComponent]
public class HitInvincibility : MonoBehaviour, IHitRecovery, IResettable
{
    [Tooltip("Seconds the player cannot be hurt after a hit. The sprite blinks meanwhile.")]
    [Min(0f)]
    [SerializeField] private float seconds = 0.5f;

    private float until;

    public bool IsInvincible { get { return Time.time < until; } }

    /// <summary>A hit just landed: the window starts now. A second hit inside it restarts it.</summary>
    public void Begin()
    {
        until = Time.time + seconds;
    }

    /// <summary>A new game starts with no window open.</summary>
    public void ResetToStart()
    {
        until = 0f;
    }
}
