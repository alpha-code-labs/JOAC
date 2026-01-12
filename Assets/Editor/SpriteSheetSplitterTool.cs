#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SpritesheetSplitterTool : EditorWindow
{
    [Header("Spritesheet Settings")]
    public Texture2D flagSpritesheet;
    public int flagWidth = 44;
    public int flagHeight = 30;
    public string outputFolder = "Assets/Resources/Flags";
    
    [Header("Options")]
    public bool createResourcesFolder = true;
    public bool overwriteExisting = true;
    public TextureFormat outputFormat = TextureFormat.RGBA32;
    
    private Dictionary<string, float> flagPositions;
    private Dictionary<string, string> codeToCountryName;
    
    [MenuItem("Tools/Split Flag Spritesheet")]
    public static void ShowWindow()
    {
        GetWindow<SpritesheetSplitterTool>("Split Flag Spritesheet");
    }
    
    void OnEnable()
    {
        InitializeFlagData();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Flag Spritesheet Splitter", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will split your flag spritesheet into individual flag images. " +
            "Each flag will be saved as a separate PNG file with the country code as filename.",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        flagSpritesheet = (Texture2D)EditorGUILayout.ObjectField(
            "Flag Spritesheet", flagSpritesheet, typeof(Texture2D), false);
        
        flagWidth = EditorGUILayout.IntField("Flag Width", flagWidth);
        flagHeight = EditorGUILayout.IntField("Flag Height", flagHeight);
        
        GUILayout.Space(5);
        
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        createResourcesFolder = EditorGUILayout.Toggle("Create Resources Folder", createResourcesFolder);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        outputFormat = (TextureFormat)EditorGUILayout.EnumPopup("Output Format", outputFormat);
        
        GUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(flagSpritesheet == null);
        
        if (GUILayout.Button("Split Spritesheet into Individual Flags", GUILayout.Height(40)))
        {
            SplitSpritesheet();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (flagSpritesheet == null)
        {
            EditorGUILayout.HelpBox("Please assign your flag spritesheet texture above.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Output files will be named: us.png, gb.png, in.png, etc.\n" +
            "These can be automatically loaded by the IndividualFlagManager.",
            MessageType.Info);
        
        if (GUILayout.Button("Create Sample Flag Manager Setup"))
        {
            CreateSampleSetup();
        }
    }
    
    void InitializeFlagData()
    {
        // Initialize flag positions (same as before)
        flagPositions = new Dictionary<string, float>
        {
            {"us", 93.38843f}, {"gb", 92.561983f}, {"in", 40.909091f}, {"ca", 14.876033f},
            {"au", 5.785124f}, {"de", 22.31405f}, {"fr", 29.752066f}, {"jp", 44.214876f},
            {"cn", 19.008264f}, {"br", 11.983471f}, {"ru", 75.206612f}, {"kr", 47.520661f},
            {"it", 42.975207f}, {"es", 26.859504f}, {"nl", 65.289256f}, {"mx", 61.570248f},
            {"ar", 4.545455f}, {"za", 98.760331f}, {"pk", 70.247934f}, {"bd", 7.85124f},
            {"lk", 50.826446f}, {"nz", 67.355372f}, {"sg", 78.099174f}, {"my", 61.983471f},
            {"th", 85.950413f}, {"ph", 69.834711f}, {"id", 39.669421f}, {"vn", 96.694215f},
            {"tr", 89.256198f}, {"eg", 25.619835f}, {"ng", 64.46281f}, {"ke", 44.628099f},
            {"gh", 31.818182f}, {"ma", 53.719008f}, {"dz", 24.380165f}, {"tn", 88.016529f},
            {"sa", 76.033058f}, {"ae", 0.826446f}, {"qa", 73.553719f}, {"kw", 47.933884f},
            {"iq", 41.735537f}, {"ir", 42.14876f}, {"af", 1.239669f}, {"il", 40.495868f},
            {"pl", 70.661157f}, {"se", 77.68595f}, {"no", 65.702479f}, {"dk", 23.140496f},
            {"fi", 27.68595f}, {"be", 8.264463f}, {"ch", 16.942149f}, {"at", 5.371901f},
            {"cz", 21.900826f}, {"hu", 39.256198f}, {"ro", 74.380165f}, {"bg", 9.090909f},
            {"gr", 34.710744f}, {"pt", 72.31405f}, {"ie", 40.082645f}, {"is", 42.561983f}
        };
        
        // Map codes to readable names
        codeToCountryName = new Dictionary<string, string>
        {
            {"us", "United States"}, {"gb", "United Kingdom"}, {"in", "India"}, {"ca", "Canada"},
            {"au", "Australia"}, {"de", "Germany"}, {"fr", "France"}, {"jp", "Japan"},
            {"cn", "China"}, {"br", "Brazil"}, {"ru", "Russia"}, {"kr", "South Korea"},
            {"it", "Italy"}, {"es", "Spain"}, {"nl", "Netherlands"}, {"mx", "Mexico"},
            {"ar", "Argentina"}, {"za", "South Africa"}, {"pk", "Pakistan"}, {"bd", "Bangladesh"},
            {"lk", "Sri Lanka"}, {"nz", "New Zealand"}, {"sg", "Singapore"}, {"my", "Malaysia"},
            {"th", "Thailand"}, {"ph", "Philippines"}, {"id", "Indonesia"}, {"vn", "Vietnam"},
            {"tr", "Turkey"}, {"eg", "Egypt"}, {"ng", "Nigeria"}, {"ke", "Kenya"},
            {"gh", "Ghana"}, {"ma", "Morocco"}, {"dz", "Algeria"}, {"tn", "Tunisia"},
            {"sa", "Saudi Arabia"}, {"ae", "United Arab Emirates"}, {"qa", "Qatar"}, {"kw", "Kuwait"},
            {"iq", "Iraq"}, {"ir", "Iran"}, {"af", "Afghanistan"}, {"il", "Israel"},
            {"pl", "Poland"}, {"se", "Sweden"}, {"no", "Norway"}, {"dk", "Denmark"},
            {"fi", "Finland"}, {"be", "Belgium"}, {"ch", "Switzerland"}, {"at", "Austria"},
            {"cz", "Czech Republic"}, {"hu", "Hungary"}, {"ro", "Romania"}, {"bg", "Bulgaria"},
            {"gr", "Greece"}, {"pt", "Portugal"}, {"ie", "Ireland"}, {"is", "Iceland"}
        };
    }
    
    void SplitSpritesheet()
    {
        if (flagSpritesheet == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a flag spritesheet texture.", "OK");
            return;
        }
        
        if (!flagSpritesheet.isReadable)
        {
            EditorUtility.DisplayDialog("Error", 
                "The spritesheet texture is not readable. Please enable 'Read/Write Enabled' in its import settings.", "OK");
            return;
        }
        
        // Create output directory
        if (createResourcesFolder && !outputFolder.Contains("Resources"))
        {
            outputFolder = "Assets/Resources/Flags";
        }
        
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }
        
        int processedCount = 0;
        int totalFlags = flagPositions.Count;
        
        foreach (var kvp in flagPositions)
        {
            string countryCode = kvp.Key;
            float yPosition = kvp.Value;
            
            // Show progress
            EditorUtility.DisplayProgressBar("Splitting Flags", 
                $"Processing {countryCode.ToUpper()}...", (float)processedCount / totalFlags);
            
            try
            {
                Texture2D flagTexture = ExtractFlag(countryCode, yPosition);
                if (flagTexture != null)
                {
                    SaveFlagTexture(flagTexture, countryCode);
                    DestroyImmediate(flagTexture);
                    processedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to process flag {countryCode}: {e.Message}");
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Complete", 
            $"Successfully split {processedCount} flags into individual images.\n" +
            $"Saved to: {outputFolder}", "OK");
        
        Debug.Log($"Flag splitting complete. {processedCount} flags saved to {outputFolder}");
    }
    
    Texture2D ExtractFlag(string countryCode, float yPosition)
    {
        // Calculate pixel position
        int spritesheetHeight = flagSpritesheet.height;
        int yPixel = Mathf.RoundToInt((yPosition / 100f) * spritesheetHeight);
        
        // Make sure we don't exceed bounds
        yPixel = Mathf.Clamp(yPixel, 0, spritesheetHeight - flagHeight);
        
        // Create new texture for this flag
        Texture2D flagTexture = new Texture2D(flagWidth, flagHeight, outputFormat, false);
        
        // Copy pixels from spritesheet (Unity uses bottom-left origin)
        Color[] pixels = flagSpritesheet.GetPixels(0, spritesheetHeight - yPixel - flagHeight, flagWidth, flagHeight);
        flagTexture.SetPixels(pixels);
        flagTexture.Apply();
        
        return flagTexture;
    }
    
    void SaveFlagTexture(Texture2D texture, string countryCode)
    {
        string filename = $"{countryCode}.png";
        string filepath = Path.Combine(outputFolder, filename);
        
        if (!overwriteExisting && File.Exists(filepath))
        {
            return; // Skip if file exists and overwrite is disabled
        }
        
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(filepath, pngData);
        
        string countryName = codeToCountryName.ContainsKey(countryCode) ? codeToCountryName[countryCode] : countryCode.ToUpper();
        Debug.Log($"Saved flag: {filename} ({countryName})");
    }
    
    void CreateSampleSetup()
    {
        // Create a sample GameObject with IndividualFlagManager
        GameObject flagManagerObj = new GameObject("FlagManager");
        IndividualFlagManager flagManager = flagManagerObj.AddComponent<IndividualFlagManager>();
        
        // Set up the manager with default settings
        flagManager.autoLoadFromResources = true;
        flagManager.flagsResourcesPath = "Flags";
        flagManager.preloadCommonFlags = true;
        
        Selection.activeGameObject = flagManagerObj;
        
        EditorUtility.DisplayDialog("Sample Created", 
            "Created a sample FlagManager GameObject with IndividualFlagManager component. " +
            "Configure the settings and assign it to your UpdateMainMenu script.", "OK");
    }
}
#endif