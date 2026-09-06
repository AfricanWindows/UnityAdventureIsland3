using UnityEngine;

/// <summary>
/// One rule of the game, written once: dying costs you the weapon you found.
///
/// It talks to IUseableWeapon, so it locks the axe, the fireball, the laser and the
/// boomerang without naming any of them, and a weapon added tomorrow is covered the moment
/// it is dropped onto the player - no edit here (Open/Closed, Dependency Inversion).
///
/// It is a SEPARATE component on purpose. The alternative - each weapon subscribing to
/// PlayerDeath itself - would repeat the same four lines in every weapon and would make
/// each of them depend on how the player dies, which is not a weapon's business
/// (Single Responsibility).
///
/// A weapon that is meant to survive death simply lives outside this object's children,
/// or the list below can grow an exception. Neither case needs a code change today.
/// </summary>
[DisallowMultipleComponent]
public class WeaponsLostOnDeath : MonoBehaviour
{
    [Tooltip("Where to look for weapons. Empty = this object and its children.")]
    [SerializeField] private Transform weaponsRoot;

    // Collected once. Doing it on every death would be a GetComponentsInChildren at the
    // worst possible moment, and the set of weapons on the player never changes at runtime.
    private IUseableWeapon[] _weapons;

    private void Awake()
    {
        Transform root = weaponsRoot != null ? weaponsRoot : transform;

        // true = include inactive, so a weapon that starts switched off is still covered.
        _weapons = root.GetComponentsInChildren<IUseableWeapon>(true);
    }

    private void OnEnable()
    {
        PlayerDeath.OnPlayerDied += LoseWeapons;
    }

    private void OnDisable()
    {
        PlayerDeath.OnPlayerDied -= LoseWeapons;
    }

    private void LoseWeapons()
    {
        for (int i = 0; i < _weapons.Length; i++)
            _weapons[i].UnEquip();

        Debug.Log("[Death] Weapons lost - pick them up again");
    }
}
