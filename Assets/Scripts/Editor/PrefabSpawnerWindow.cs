using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Places and removes level tiles directly in the Scene view.
/// Left click adds the selected prefab, right click deletes the object under the cursor.
///
/// The drop down lists every prefab in Assets/Resources/Tiles, so dropping a new prefab
/// into that folder is all it takes for it to appear here. Spawn Parent decides where the
/// tiles land in the Hierarchy: empty = the scene root, or drag a container such as World.
/// </summary>
public class PrefabSpawnerWindow : EditorWindow
{
    private const string TilesFolder = "Assets/Resources/Tiles";

    private static bool _isSpawningEnabled = false;

    // SerializeField, not a plain field: an EditorWindow is a ScriptableObject, and Unity
    // wipes unserialised state on every script recompile. Without this the chosen parent
    // would silently reset to null in the middle of building a level.
    [SerializeField] private Transform _spawnParent;

    private int _selectedIndex = 0;
    private GUIStyle _labelStyle;
    private Dictionary<string, GameObject> _prefabDictionary;

    // Filled by LoadPrefabs from the folder itself - no hand-kept array of names.
    private string[] _dropDownOptions = new string[0];

    [MenuItem("Tools/Prefab Spawner")]
    public static void ShowWindow()
    {
       var window = GetWindow<PrefabSpawnerWindow>();
       window.titleContent = new GUIContent("Prefab Spawner");
       window.Show();
    }

    private void OnEnable()
    {
        _labelStyle = new GUIStyle();
        _labelStyle.normal.textColor = Color.white;

        SceneView.duringSceneGui += OnSceneGUI;

        LoadPrefabs();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        if(_dropDownOptions.Length == 0)
        {
            EditorGUILayout.HelpBox("No prefab was found in " + TilesFolder + ".", MessageType.Warning);
            if(GUILayout.Button("Refresh List"))
                LoadPrefabs();

            return;
        }

        _selectedIndex = EditorGUILayout.Popup("Select Option",_selectedIndex,_dropDownOptions);

        // allowSceneObjects: true is the whole point - this must accept an object from the
        // Hierarchy, not from the Project window.
        _spawnParent = EditorGUILayout.ObjectField(
            new GUIContent("Spawn Parent",
                           "Drag a scene object here and every tile is created inside it. " +
                           "Leave empty to place tiles at the root of the Hierarchy."),
            _spawnParent, typeof(Transform), true) as Transform;

        EditorGUILayout.LabelField(" ", _spawnParent != null
            ? "New tiles go into: " + _spawnParent.name
            : "New tiles go to the scene root.");

        EditorGUILayout.Space();

        if (GUILayout.Button("Toggle Prefab Spawning"))
            TogglePrefabSpawning();

        if (GUILayout.Button("Refresh List"))
            LoadPrefabs();

        GUILayout.Label("Prefab Spawning Status: " + (_isSpawningEnabled ? "<color=yellow>Enabled</color>" : "<color=red>Disabled</color>"), _labelStyle);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(_isSpawningEnabled
            ? "Left click in the Scene = add the selected prefab.\nRight click in the Scene = delete the object under the cursor.\nNormal selection is off while this is Enabled."
            : "Spawning is off. Press Toggle Prefab Spawning to start placing tiles.",
            MessageType.Info);
    }

    private void TogglePrefabSpawning()
    {
       _isSpawningEnabled = !_isSpawningEnabled;
       SceneView.RepaintAll();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if(!_isSpawningEnabled || _prefabDictionary == null)
            return;

        Event current = Event.current;

        // Take over the mouse, otherwise Unity selects objects on left click and opens
        // its context menu on right click before we ever see the event.
        if(current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if(current.type != EventType.MouseDown)
            return;

        if(current.button == 0)
        {
            SpawnAt(GetMouseCell(current.mousePosition));
            current.Use();
        }
        else if(current.button == 1)
        {
            DeleteAt(current.mousePosition);
            current.Use();
        }
    }

    /// <summary>Mouse position snapped to the tile grid.</summary>
    private Vector3 GetMouseCell(Vector2 mousePosition)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        Vector3 world = ray.origin;

        return new Vector3(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y), 0);
    }

    private void SpawnAt(Vector3 position)
    {
        if(_selectedIndex >= _dropDownOptions.Length)
            return;

        GameObject prefab = _prefabDictionary[_dropDownOptions[_selectedIndex]];
        Transform parent = ResolveSpawnParent();

        // PrefabUtility keeps the link to the prefab, plain Instantiate does not - without
        // it, a later edit of the prefab would never reach the tiles already placed.
        //
        // The parent goes in at CREATION time rather than through a SetParent afterwards:
        // Undo.RegisterCreatedObjectUndo then covers the whole thing in one step, so Ctrl+Z
        // removes the object instead of leaving an orphan behind at the scene root.
        GameObject spawned = parent != null
            ? PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject
            : PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if(spawned == null)
            return;

        // World position, set AFTER parenting, so the tile lands under the mouse whatever
        // the container's own transform happens to be.
        spawned.transform.position = position;

        Undo.RegisterCreatedObjectUndo(spawned,"Spawn " + spawned.name);
        Selection.activeGameObject = spawned;
    }

    /// <summary>
    /// Where new tiles go: the container the user dropped in, or null for the scene root.
    ///
    /// It knows nothing about what is being spawned, so it works for floors, enemies,
    /// fruit and anything added later - the container is just a Transform (Open/Closed).
    ///
    /// A prefab ASSET dragged in by mistake is rejected here rather than at Instantiate:
    /// parenting a scene object to an asset is not a thing, and the failure would otherwise
    /// look like "spawning stopped working".
    /// </summary>
    private Transform ResolveSpawnParent()
    {
        if(_spawnParent == null)
            return null;

        if(EditorUtility.IsPersistent(_spawnParent))
        {
            Debug.LogWarning("Prefab Spawner: Spawn Parent is a project asset, not a scene " +
                             "object. Spawning at the scene root instead.");
            return null;
        }

        return _spawnParent;
    }

    private void DeleteAt(Vector2 mousePosition)
    {
        GameObject picked = HandleUtility.PickGameObject(mousePosition,false);
        if(picked == null)
        {
            Debug.Log("Prefab Spawner: nothing to delete under the cursor.");
            return;
        }

        // Delete the whole tile, not just the child part that happened to be clicked.
        GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(picked);
        if(root == null)
            root = picked;

        Debug.Log("Prefab Spawner: deleted " + root.name);
        Undo.DestroyObjectImmediate(root);
    }

    /// <summary>
    /// Every prefab that sits in Assets/Resources/Tiles, in alphabetical order.
    ///
    /// The folder is the source of truth: no hand-kept array of tile names to update, so a
    /// prefab appears in the drop down the moment it is dropped into the folder and the
    /// list is refreshed (Open/Closed).
    /// </summary>
    private void LoadPrefabs()
    {
        _prefabDictionary = new Dictionary<string, GameObject>();
        List<string> loadedNames = new List<string>();

        // AssetDatabase, not Resources.LoadAll: this is editor-only code, and the folder
        // is the source of truth here - not what happens to be loadable at runtime.
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { TilesFolder });

        for(int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if(prefab == null || _prefabDictionary.ContainsKey(prefab.name))
                continue;

            _prefabDictionary.Add(prefab.name, prefab);
            loadedNames.Add(prefab.name);
        }

        // FindAssets returns them in whatever order the database feels like, which would
        // reshuffle the drop down after every reimport.
        loadedNames.Sort(System.StringComparer.OrdinalIgnoreCase);

        _dropDownOptions = loadedNames.ToArray();
        _selectedIndex = Mathf.Clamp(_selectedIndex,0,Mathf.Max(0,_dropDownOptions.Length - 1));
    }
}
