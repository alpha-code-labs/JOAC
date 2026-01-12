using UnityEngine;
using UnityEditor;
using System.IO;

public class BallMaterialUpdater : EditorWindow
{
    private string folderPath = "Assets/Resources/Sequences/Batsman"; // Your prefab folder
    private Material newMaterial;
    private Vector3 newScale = Vector3.one;

    [MenuItem("Tools/Update BALL Objects in Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<BallMaterialUpdater>("Update BALL Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("Update BALL Objects in Prefabs", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Prefab Folder Path", folderPath);
        newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), false);
        newScale = EditorGUILayout.Vector3Field("New Local Scale", newScale);

        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChangesToPrefabs();
        }
    }

    private void ApplyChangesToPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            bool modified = false;
            foreach (Transform child in prefabInstance.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.ToLower().Contains("ball"))
                {
                    //Update Material
                    Renderer renderer = child.GetComponent<Renderer>();
                    if (renderer != null && newMaterial != null)
                    {
                        Undo.RecordObject(renderer, "Change Material");
                        renderer.sharedMaterial = newMaterial;
                    }
                    //Update Scale
                    Undo.RecordObject(child.transform, "Change Scale");
                    child.localScale = newScale;

                    modified = true;
                    Debug.Log($"Updated '{child.name}' in prefab: {assetPath}");
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
            }

            DestroyImmediate(prefabInstance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished updating prefabs.");
    }
}
