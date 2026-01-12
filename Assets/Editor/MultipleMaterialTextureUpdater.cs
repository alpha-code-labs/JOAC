using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MultipleMaterialTextureUpdater : EditorWindow
{
    [System.Serializable]
    public class MaterialTextureMapping
    {
        public string materialNamePrefix;
        public Material newMaterial;
        public Texture2D newTexture;
        public string texturePropertyName = "_MainTex";
        public bool enabled = true;
        public bool swapEntireMaterial = true; // New option to choose between material swap or texture swap

        public MaterialTextureMapping(string namePrefix)
        {
            materialNamePrefix = namePrefix;
        }
    }

    private string folderPath = "Assets/Resources/Sequences/Batsman";
    private List<MaterialTextureMapping> materialMappings = new List<MaterialTextureMapping>();
    private Vector2 scrollPosition;
    private bool showAddMappingSection = false;
    private string newMappingName = "";

    [MenuItem("Tools/Update Multiple Material Textures by Name")]
    public static void ShowWindow()
    {
        MultipleMaterialTextureUpdater window = GetWindow<MultipleMaterialTextureUpdater>("Multi-Material Texture Updater");
        window.InitializeDefaultMappings();
    }

    private void InitializeDefaultMappings()
    {
        if (materialMappings.Count == 0)
        {
            // Add some default mappings - modify these to match your material name prefixes
            materialMappings.Add(new MaterialTextureMapping("Body"));
            materialMappings.Add(new MaterialTextureMapping("FootWear"));
            materialMappings.Add(new MaterialTextureMapping("Head"));
            materialMappings.Add(new MaterialTextureMapping("OutfitTop"));
            materialMappings.Add(new MaterialTextureMapping("OutfitBottom"));
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Multiple Material Texture Updater", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Folder path
        folderPath = EditorGUILayout.TextField("Prefab Folder Path", folderPath);
        EditorGUILayout.Space();

        // Material mappings section
        GUILayout.Label("Material Name Prefix Mappings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Define material name prefixes and their replacements. Materials starting with these names will be updated.", MessageType.Info);

        // Draw existing mappings
        for (int i = 0; i < materialMappings.Count; i++)
        {
            DrawMaterialMapping(materialMappings[i], i);
        }

        EditorGUILayout.Space();

        // Add new mapping section
        if (GUILayout.Button(showAddMappingSection ? "Hide Add Mapping" : "Add New Mapping"))
        {
            showAddMappingSection = !showAddMappingSection;
        }

        if (showAddMappingSection)
        {
            EditorGUILayout.BeginVertical("box");
            newMappingName = EditorGUILayout.TextField("New Mapping Name", newMappingName);

            if (GUILayout.Button("Create Mapping") && !string.IsNullOrEmpty(newMappingName))
            {
                materialMappings.Add(new MaterialTextureMapping(newMappingName));
                newMappingName = "";
                showAddMappingSection = false;
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        // Action buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Apply All Enabled Mappings", GUILayout.Height(30)))
        {
            ApplyAllMappings();
        }

        if (GUILayout.Button("Preview Materials in Folder"))
        {
            PreviewMaterialsInFolder();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Utility buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Enable All"))
        {
            foreach (var mapping in materialMappings)
                mapping.enabled = true;
        }

        if (GUILayout.Button("Disable All"))
        {
            foreach (var mapping in materialMappings)
                mapping.enabled = false;
        }

        if (GUILayout.Button("Clear All Mappings"))
        {
            if (EditorUtility.DisplayDialog("Clear All Mappings", "Are you sure you want to clear all material mappings?", "Yes", "Cancel"))
            {
                materialMappings.Clear();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void DrawMaterialMapping(MaterialTextureMapping mapping, int index)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        mapping.enabled = EditorGUILayout.Toggle(mapping.enabled, GUILayout.Width(20));
        mapping.materialNamePrefix = EditorGUILayout.TextField("Material Name Prefix", mapping.materialNamePrefix);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            materialMappings.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!mapping.enabled);

        // Toggle between material swap and texture swap
        mapping.swapEntireMaterial = EditorGUILayout.Toggle("Swap Entire Material", mapping.swapEntireMaterial);

        if (mapping.swapEntireMaterial)
        {
            mapping.newMaterial = (Material)EditorGUILayout.ObjectField("New Material", mapping.newMaterial, typeof(Material), false);
        }
        else
        {
            mapping.newTexture = (Texture2D)EditorGUILayout.ObjectField("New Texture", mapping.newTexture, typeof(Texture2D), false);
            mapping.texturePropertyName = EditorGUILayout.TextField("Texture Property", mapping.texturePropertyName);
        }

        // Show preview of matching materials
        if (!string.IsNullOrEmpty(mapping.materialNamePrefix))
        {
            EditorGUILayout.LabelField("Preview matching materials:", EditorStyles.miniLabel);
            ShowMatchingMaterials(mapping.materialNamePrefix);
        }

        // Validation
        bool isValid = !string.IsNullOrEmpty(mapping.materialNamePrefix) &&
                      ((mapping.swapEntireMaterial && mapping.newMaterial != null) ||
                       (!mapping.swapEntireMaterial && mapping.newTexture != null));

        if (!isValid && mapping.enabled)
        {
            string missingItem = mapping.swapEntireMaterial ? "material" : "texture";
            EditorGUILayout.HelpBox($"Missing material name prefix or {missingItem} assignment!", MessageType.Warning);
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void ShowMatchingMaterials(string namePrefix)
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        HashSet<string> matchingMaterialNames = new HashSet<string>();

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterials != null)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null && mat.name.StartsWith(namePrefix, System.StringComparison.OrdinalIgnoreCase))
                        {
                            matchingMaterialNames.Add(mat.name);
                        }
                    }
                }
            }
        }

        if (matchingMaterialNames.Count > 0)
        {
            string preview = string.Join(", ", matchingMaterialNames);
            if (preview.Length > 100) preview = preview.Substring(0, 100) + "...";
            EditorGUILayout.LabelField($"  Found: {preview}", EditorStyles.wordWrappedMiniLabel);
        }
        else
        {
            EditorGUILayout.LabelField("  No matching materials found", EditorStyles.wordWrappedMiniLabel);
        }
    }

    private void ApplyAllMappings()
    {
        // Validate enabled mappings
        List<MaterialTextureMapping> validMappings = new List<MaterialTextureMapping>();

        foreach (var mapping in materialMappings)
        {
            if (mapping.enabled)
            {
                bool hasValidAssignment = mapping.swapEntireMaterial ?
                    mapping.newMaterial != null :
                    mapping.newTexture != null;

                if (string.IsNullOrEmpty(mapping.materialNamePrefix) || !hasValidAssignment)
                {
                    string missingItem = mapping.swapEntireMaterial ? "material" : "texture";
                    Debug.LogError($"Mapping with prefix '{mapping.materialNamePrefix}' is missing name prefix or {missingItem} assignment!");
                    return;
                }

                validMappings.Add(mapping);
            }
        }

        if (validMappings.Count == 0)
        {
            Debug.LogWarning("No valid enabled mappings found!");
            return;
        }

        // Apply mappings
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        int updatedPrefabs = 0;
        int totalObjectsUpdated = 0;
        Dictionary<string, int> mappingCounts = new Dictionary<string, int>();

        // Initialize counters
        foreach (var mapping in validMappings)
        {
            mappingCounts[mapping.materialNamePrefix] = 0;
        }

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            bool prefabModified = false;
            Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterials != null)
                {
                    Material[] materials = renderer.sharedMaterials;
                    bool rendererModified = false;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            // Check each mapping
                            foreach (var mapping in validMappings)
                            {
                                if (materials[i].name.StartsWith(mapping.materialNamePrefix, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    if (mapping.swapEntireMaterial)
                                    {
                                        // Swap entire material
                                        Undo.RecordObject(renderer, "Change Material");
                                        materials[i] = mapping.newMaterial;

                                        rendererModified = true;
                                        totalObjectsUpdated++;
                                        mappingCounts[mapping.materialNamePrefix]++;

                                        Debug.Log($"Swapped material '{materials[i].name}' -> '{mapping.newMaterial.name}' on '{renderer.name}' in prefab: {assetPath}");
                                    }
                                    else
                                    {
                                        // Swap just the texture
                                        if (!materials[i].HasProperty(mapping.texturePropertyName))
                                        {
                                            Debug.LogWarning($"Material '{materials[i].name}' doesn't have property '{mapping.texturePropertyName}', skipping...");
                                            continue;
                                        }

                                        // Create new material instance
                                        Material newMat = new Material(materials[i]);
                                        newMat.SetTexture(mapping.texturePropertyName, mapping.newTexture);

                                        Undo.RecordObject(renderer, "Change Material Texture");
                                        materials[i] = newMat;

                                        rendererModified = true;
                                        totalObjectsUpdated++;
                                        mappingCounts[mapping.materialNamePrefix]++;

                                        Debug.Log($"Applied texture to material '{materials[i].name}' on '{renderer.name}' in prefab: {assetPath}");
                                    }

                                    break; // Only apply first matching mapping
                                }
                            }
                        }
                    }

                    if (rendererModified)
                    {
                        renderer.sharedMaterials = materials;
                        prefabModified = true;
                    }
                }
            }

            if (prefabModified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                updatedPrefabs++;
            }

            DestroyImmediate(prefabInstance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Log results
        Debug.Log($"=== MATERIAL UPDATE COMPLETE ===");
        Debug.Log($"Updated {totalObjectsUpdated} objects in {updatedPrefabs} prefabs");
        Debug.Log("Mapping Results:");
        foreach (var kvp in mappingCounts)
        {
            Debug.Log($"  - {kvp.Key}: {kvp.Value} objects updated");
        }
    }

    private void PreviewMaterialsInFolder()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        Dictionary<Material, int> materialCounts = new Dictionary<Material, int>();

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterials != null)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            if (materialCounts.ContainsKey(mat))
                                materialCounts[mat]++;
                            else
                                materialCounts[mat] = 1;
                        }
                    }
                }
            }
        }

        Debug.Log($"=== MATERIALS FOUND IN FOLDER ===");
        Debug.Log($"Found {materialCounts.Count} unique materials:");
        foreach (var kvp in materialCounts)
        {
            Debug.Log($"- {kvp.Key.name} (Used {kvp.Value} times) - Shader: {kvp.Key.shader.name}");
        }
    }
}