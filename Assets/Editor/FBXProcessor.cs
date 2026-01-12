using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System;

public class FBXProcessor : EditorWindow
{
    private string relativeFolderPath = "Assets/Cutscene FBX Batsman2"; // CHANGE THIS

    [MenuItem("Tools/Process Batsman FBX Animations")]
    public static void ShowWindow()
    {
        GetWindow<FBXProcessor>("FBX Animation Processor");
    }

    void OnGUI()
    {
        GUILayout.Label("FBX Animation Processor", EditorStyles.boldLabel);
        relativeFolderPath = EditorGUILayout.TextField("FBX Folder Path", relativeFolderPath);

        if (GUILayout.Button("Process FBX Files"))
        {
            ProcessFBXFiles(relativeFolderPath);
        }
    }

    private static void ProcessFBXFiles(string folderPath)
    {
        string centralPrefabFolder = "Assets/Resources/Sequences/Batsman2";

        // Create the central folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(centralPrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ProcessedPrefabs");
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string fileFolder = Path.GetDirectoryName(assetPath);
            string newFolderPath = Path.Combine(fileFolder, fileName);

            // Create subfolder next to FBX if needed
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                AssetDatabase.CreateFolder(fileFolder, fileName);
            }

            // Load animation clip from FBX
            GameObject fbxGO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            AnimationClip sceneClip = null;
            foreach (var obj in assets)
            {
                if (obj is AnimationClip clip && !clip.name.Contains("__preview__"))
                {
                    sceneClip = clip;
                    break;
                }
            }

            if (sceneClip == null)
            {
                Debug.LogWarning($"❌ No scene animation found in {assetPath}");
                continue;
            }

            // Create and save animation clip
            string clipPath = $"{newFolderPath}/{sceneClip.name}.anim";
            AnimationClip newClip = UnityEngine.Object.Instantiate(sceneClip);
            AssetDatabase.CreateAsset(newClip, clipPath);

            // Create and save Animator Controller
            string controllerPath = $"{newFolderPath}/{fileName}_Controller.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddMotion(newClip);

            // Instantiate, assign controller, and save prefab in subfolder
            GameObject newGO = UnityEngine.Object.Instantiate(fbxGO);
            newGO.name = fileName;

            Animator animator = newGO.GetComponent<Animator>();
            if (animator == null)
                animator = newGO.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;

            string prefabLocalPath = $"{newFolderPath}/{fileName}_Prefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(newGO, prefabLocalPath);
            UnityEngine.Object.DestroyImmediate(newGO);

            // Copy prefab to central folder
            string centralPrefabPath = $"{centralPrefabFolder}/{fileName}.prefab";
            AssetDatabase.CopyAsset(prefabLocalPath, centralPrefabPath);

            Debug.Log($"✅ Processed {fileName}, prefab saved to:\n→ {prefabLocalPath}\n→ {centralPrefabPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("🎉 All FBX files processed and prefabs copied.");
    }

}
