using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BuildLevel : EditorWindow
{
    private TextAsset _curLevel;
    private GameObject _world;
    private bool _showPalette = true;

    [MenuItem("Tools/Level Creator")]
    public static void ShowWindow()
    {
        Debug.Log("Level Creator");
        GetWindow<BuildLevel>("Level Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Assign Level File:");
        _curLevel = EditorGUILayout.ObjectField(_curLevel,typeof(TextAsset),false) as TextAsset;

        EditorGUILayout.LabelField("Assign Parent Transform");
        _world = EditorGUILayout.ObjectField(_world,typeof(GameObject),true) as GameObject;

        if(GUILayout.Button("Create Level") && _curLevel != null &&  _world != null)
        {
            CreateLevel();
        }

        EditorGUILayout.Space();
        DrawPaletteTable();
    }

    /// <summary>
    /// Shows which tile code creates which prefab. Tiled numbers the tiles by their
    /// position in the tileset image (left to right, top to bottom, starting at 1),
    /// so the images in MarioTileSet must be placed in exactly this order.
    /// </summary>
    private void DrawPaletteTable()
    {
        _showPalette = EditorGUILayout.Foldout(_showPalette,"Tile Codes (order of the Tiled tileset)",true);
        if(!_showPalette)
            return;

        string[] names = TilePalette.GetAllPrefabNames();

        EditorGUI.indentLevel++;
        for(int i = 0; i < names.Length; i++)
        {
            bool exists = Resources.Load<GameObject>("Tiles/" + names[i]) != null;
            string label = names[i] + (exists ? "" : "   (prefab missing)");

            EditorGUILayout.LabelField(new GUIContent((i + 1).ToString()),new GUIContent(label));
        }
        EditorGUI.indentLevel--;
    }

    private void CreateLevel()
    {
        try
        {
            Debug.Log("Creating Level: " + _curLevel.name);
            string jsonData = _curLevel.text;
            Dictionary<string,object> gameData = MiniJSON.Json.Deserialize(jsonData) as Dictionary<string,object>;
            int height = int.Parse(gameData["height"].ToString());
            int width = int.Parse(gameData["width"].ToString());
            Debug.Log(height + " " + width);

            List<object> layers = (List<object>)gameData["layers"];
            foreach(object obj in layers)
            {
                Dictionary<string,object> layerData = (Dictionary<string,object>)obj;
                if(layerData.ContainsKey("data"))
                {
                    List<object> levelTiles = (List<object>)layerData["data"];
                    Debug.Log(levelTiles.Count);
                    for(int i=0; i < levelTiles.Count;i++)
                    {
                        int tileCode;
                        if(!int.TryParse(levelTiles[i].ToString(), out tileCode))
                            continue;

                        string prefabName = TilePalette.GetPrefabName(tileCode);
                        if(prefabName != null)
                            CreateGameObject(prefabName,i,height,width);
                    }
                }
            }
        }
        catch(Exception e)
        {
            // LogException keeps the stack trace and shows up as an error, not as a plain log.
            Debug.LogException(e);
        }
    }

    private void CreateGameObject(string prefabName,int index, int height, int width)
    {
        try
        {
            GameObject prefab = Resources.Load<GameObject>("Tiles/" + prefabName);
            if(prefab == null)
            {
                Debug.LogError("Prefab not found: Assets/Resources/Tiles/" + prefabName + ".prefab");
                return;
            }

            // PrefabUtility keeps the link to the prefab, plain Instantiate does not.
            // Without the link, later edits of Prefab_Mario would never reach the level.
            GameObject temp = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if(temp == null)
                return;

            Undo.RegisterCreatedObjectUndo(temp,"Create Level");

            int colCalc = index % width;
            string col = colCalc.ToString();
            if(colCalc < 10)
                col = "0" + colCalc;

            int rowCalc = (int)((height - 1) - ((int)(index / width)));
            string row = rowCalc.ToString();
               if(rowCalc < 10)
                row = "0" + rowCalc;

            temp.name = row + col;
            temp.transform.localPosition = new Vector3(colCalc,rowCalc,0);
            temp.transform.SetParent(_world.transform);
        }
        catch(Exception e)
        {
            // LogException keeps the stack trace and shows up as an error, not as a plain log.
            Debug.LogException(e);
        }
    }
}
