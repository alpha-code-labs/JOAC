using UnityEngine;
using UnityEditor;
using System.IO;

public class BallMaterialUpdaterForFieldingAssets : EditorWindow
{
    private string folderPath = "Assets/Resources/Sequences/Fielding"; // Your prefab folder
    private Material newMaterial;
    private Vector3 newScale = Vector3.one;

    [MenuItem("Tools/Update BALL Objects in Fielding Assets")]
    public static void ShowWindow()
    {
        GetWindow<BallMaterialUpdaterForFieldingAssets>("Update BALL Objects in Fielding Assets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Update BALL Objects in Fielding Assets", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Fielding Assets Folder Path", folderPath);
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
            Transform firstChild = prefabInstance.transform.childCount > 0 ? prefabInstance.transform.GetChild(0) : null;

            if (firstChild != null)
            {
                // Update Material
                Renderer renderer = firstChild.GetComponent<Renderer>();
                if (renderer != null && newMaterial != null)
                {
                    Undo.RecordObject(renderer, "Change Material");
                    renderer.sharedMaterial = newMaterial;
                }

                // Update Scale
                Undo.RecordObject(firstChild.transform, "Change Scale");
                firstChild.localScale = newScale;

                modified = true;
                Debug.Log($"Updated first child '{firstChild.name}' in prefab: {assetPath}");
            }
            else
            {
                Debug.LogWarning($"No child found in prefab: {assetPath}");
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
