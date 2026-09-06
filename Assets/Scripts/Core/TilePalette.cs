/// <summary>
/// Level tile palette: the array index IS the tile code used in the level JSON file.
/// Both Level Creator and Prefab Spawner read prefab names only from here,
/// so adding a new tile is ONE line and no logic has to change (Open/Closed Principle).
/// </summary>
public static class TilePalette
{
    public const int EmptyTile = 0;

    private static readonly string[] PrefabNames =
    {
        null,                 //  0 - empty cell
        "Prefab_Floor",       //  1
        "Prefab_Mario",       //  2
        "Prefab_Coin",        //  3
        "Prefab_Flower",      //  4
        "Prefab_Star",        //  5
        "Prefab_Spikes",      //  6
        "Prefab_Axe",         //  7
        "Prefab_Key",         //  8
        "Prefab_Heart",       //  9 - exercise 2: the heart that gives a health point
        "Prefab_Door",        // 10
        "Prefab_Goomba",      // 11
        "Prefab_Bowser",      // 12
        "Prefab_Lightning",   // 13 - exercise 1: +50% speed for 5 seconds
        "Prefab_BlinkTile",   // 14 - exercise 3: appears/disappears every 2 seconds
        "Prefab_MovingTile",  // 15 - exercise 4: moves 2 tiles right and 2 tiles left
        "Prefab_LaserPowerUp", // 16 - exercise 3 (hw3): unlocks the laser gun
        "Prefab_FlyingFish",   // 17 - static enemy: hangs in the air, kills Mario on touch
        "Prefab_BoomerangPowerUp" // 18 - unlocks the boomerang gun
    };

    /// <summary>Prefab name for a tile code, or null when the code is not in the palette.</summary>
    public static string GetPrefabName(int tileCode)
    {
        if (tileCode <= EmptyTile || tileCode >= PrefabNames.Length)
            return null;

        return PrefabNames[tileCode];
    }

    /// <summary>Every prefab name in the palette, without the empty cell.</summary>
    public static string[] GetAllPrefabNames()
    {
        string[] names = new string[PrefabNames.Length - 1];
        for (int i = 1; i < PrefabNames.Length; i++)
            names[i - 1] = PrefabNames[i];

        return names;
    }
}
