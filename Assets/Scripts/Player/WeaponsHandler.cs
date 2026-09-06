using System.Collections.Generic;
using Game.Core.Controls;
using UnityEngine;

/// <summary>
/// Collects every IWeapon found under the player and lets him switch and fire.
///
/// New weapons are registered automatically and get their own number slot, so adding
/// one needs no change in this class (Open/Closed) - only a new component on the player.
///
/// It knows nothing about keys any more: "slot 3 was chosen" and "the trigger was pulled"
/// arrive through IInputSource. Which physical key means what moved into
/// KeyboardInputSource, the only class in the project that should own it
/// (Single Responsibility).
/// </summary>
public class WeaponsHandler : InputDrivenBehaviour
{
    [Tooltip("Where to look for weapons. Empty = this object's parent (the player).")]
    [SerializeField] private Transform weaponsRoot;

    private List<IWeapon> weapons = new List<IWeapon>();
    private int index = 0;

    private void Awake()
    {
        weapons = new List<IWeapon>();
        CollectWeapons();
    }

    public void AddWeapon(IWeapon weapon)
    {
        if (weapon != null && !weapons.Contains(weapon))
            weapons.Add(weapon);
    }

    public void SelectWeapon(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weapons.Count)
            return;

        index = newIndex;
        Debug.Log("Selected weapon " + (index + 1) + ": " + weapons[index].GetType().Name);
    }

    private void Update()
    {
        if (!HasInput)
            return;

        ReadSelection();

        if (InputSource.AttackPressed)
            FireSelected();
    }

    /// <summary>
    /// Pulls the trigger on the selected weapon.
    ///
    /// It deliberately does NOT ask whether that weapon is unlocked. A weapon that still
    /// needs its power-up answers with its own message (see Game.Weapons.BaseWeapon), so
    /// this class never has to learn that a laser, or a power-up, exists.
    /// </summary>
    private void FireSelected()
    {
        if (index < 0 || index >= weapons.Count)
            return;

        weapons[index].Attack();
    }

    /// <summary>
    /// Asks the input source about as many slots as there are weapons - never more, so a
    /// device with nine buttons and a player with two weapons costs two questions.
    /// </summary>
    private void ReadSelection()
    {
        int slots = Mathf.Min(InputSource.WeaponSlotCount, weapons.Count);

        for (int i = 0; i < slots; i++)
        {
            if (InputSource.WeaponSelectPressed(i))
            {
                SelectWeapon(i);
                return;
            }
        }
    }

    private void CollectWeapons()
    {
        Transform root = weaponsRoot;
        if (root == null)
            root = transform.parent != null ? transform.parent : transform;

        IWeapon[] found = root.GetComponentsInChildren<IWeapon>(true);
        for (int i = 0; i < found.Length; i++)
            AddWeapon(found[i]);

        Debug.Log("WeaponsHandler: found " + weapons.Count + " weapon(s) under " + root.name);
    }
}
