using UnityEngine;

/// <summary>
/// Where the power bar's numbers actually live: once, in one asset on disk.
///
/// The alternative - fields on the player prefab - means re-balancing the game requires
/// opening a prefab every time. With a ScriptableObject the values exist exactly once and
/// a designer edits them without opening a scene or a prefab.
///
/// There is one asset today - Assets/Settings/PowerConfig - and re-balancing the game
/// means editing the numbers in it. If a second difficulty is ever wanted, it is a second
/// asset dropped into the same field: same class, same controller, no new code.
///
/// This is the same pattern the weapons already use (ProjectileConfigSO), on purpose -
/// one way of doing tunable numbers in this project, not two.
/// </summary>
[CreateAssetMenu(fileName = "PowerConfig", menuName = "Player/Power Config")]
public class PowerConfigSO : ScriptableObject
{
    [SerializeField] private PowerStats stats = new PowerStats(15, 15, 3f);

    public PowerStats Stats { get { return stats; } }
}
