using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Removes "Missing (Mono Script)" components left behind when a script file is deleted.
/// Unity refuses to save a prefab while such a component is on it, and the component
/// header is drawn empty, so it is easy to miss with a right click.
/// </summary>
public static class MissingScriptCleaner
{
    [MenuItem("Tools/Remove Missing Scripts From Selection")]
    private static void RemoveFromSelection()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("Select a GameObject in the Hierarchy first.");
            return;
        }

        int removed = 0;

        foreach (GameObject root in selected)
        {
            // Children too - the missing script is often on a child object.
            Transform[] all = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in all)
            {
                GameObject target = t.gameObject;

                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(target) == 0)
                    continue;

                Undo.RegisterCompleteObjectUndo(target, "Remove Missing Scripts");
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target);
                EditorUtility.SetDirty(target);

                Debug.Log("Cleaned: " + target.name);
            }
        }

        // Mark the open prefab as changed, otherwise Save stays greyed out.
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
            EditorSceneManager.MarkSceneDirty(stage.scene);

        Debug.Log("Removed " + removed + " missing script component(s).");
    }
}
