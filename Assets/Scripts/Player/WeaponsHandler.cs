using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Collects every IWeapon found under the player and lets him switch and fire.
///
/// New weapons are registered automatically and get their own number key, so adding
/// one needs no change in this class (OCP) - only a new component on the player.
/// </summary>
public class WeaponsHandler : MonoBehaviour
{
    // Number keys, in order. Weapon 1 answers to Digit1, weapon 2 to Digit2, and so on.
    private static readonly Key[] SelectionKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
    };

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
        if (Keyboard.current == null)
            return;

        ReadSelectionKeys();

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
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

    private void ReadSelectionKeys()
    {
        int keyCount = Mathf.Min(SelectionKeys.Length, weapons.Count);

        for (int i = 0; i < keyCount; i++)
        {
            if (Keyboard.current[SelectionKeys[i]].wasPressedThisFrame)
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
