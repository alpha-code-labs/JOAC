using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CountryFlagData
{
    public string countryName;
    public string countryCode; // ISO 2-letter code
    public Sprite flagSprite;

    public CountryFlagData(string name, string code, Sprite sprite)
    {
        countryName = name;
        countryCode = code;
        flagSprite = sprite;
    }
}

public class IndividualFlagManager : MonoBehaviour
{
    [Header("Flag Collection")]
    public List<CountryFlagData> flagDatabase = new List<CountryFlagData>();
    public Sprite defaultFlag; // Fallback for unknown countries

    [Header("Auto-Load Settings")]
    public bool autoLoadFromResources = true;
    public string flagsResourcesPath = "Flags"; // Resources/Flags/ folder

    [Header("Performance")]
    public bool cacheFlags = true;
    public bool preloadCommonFlags = true;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Dictionary<string, Sprite> flagCache = new Dictionary<string, Sprite>();
    private Dictionary<string, string> countryNameToCode = new Dictionary<string, string>();

    // Common countries to preload for better performance
    private readonly string[] commonCountries = {
        "United States", "India", "United Kingdom", "Canada", "Australia",
        "Germany", "France", "Japan", "China", "Brazil", "Russia",
        "Pakistan", "Bangladesh", "Sri Lanka", "South Africa", "New Zealand"
    };

    void Start()
    {
        InitializeCountryMapping();

        if (autoLoadFromResources)
        {
            LoadFlagsFromResources();
        }

        BuildFlagDatabase();

        if (preloadCommonFlags)
        {
            PreloadCommonFlags();
        }

        if (showDebugLogs)
        {
            Debug.Log($"Flag Manager initialized with {flagDatabase.Count} flags");
        }
    }

    void InitializeCountryMapping()
    {
        // Map country names to ISO 2-letter codes for file naming
        countryNameToCode.Add("Afghanistan", "af");
        countryNameToCode.Add("Albania", "al");
        countryNameToCode.Add("Algeria", "dz");
        countryNameToCode.Add("Argentina", "ar");
        countryNameToCode.Add("Armenia", "am");
        countryNameToCode.Add("Australia", "au");
        countryNameToCode.Add("Austria", "at");
        countryNameToCode.Add("Azerbaijan", "az");
        countryNameToCode.Add("Bahrain", "bh");
        countryNameToCode.Add("Bangladesh", "bd");
        countryNameToCode.Add("Belarus", "by");
        countryNameToCode.Add("Belgium", "be");
        countryNameToCode.Add("Bolivia", "bo");
        countryNameToCode.Add("Brazil", "br");
        countryNameToCode.Add("Bulgaria", "bg");
        countryNameToCode.Add("Cambodia", "kh");
        countryNameToCode.Add("Canada", "ca");
        countryNameToCode.Add("Chile", "cl");
        countryNameToCode.Add("China", "cn");
        countryNameToCode.Add("Colombia", "co");
        countryNameToCode.Add("Croatia", "hr");
        countryNameToCode.Add("Czech Republic", "cz");
        countryNameToCode.Add("Denmark", "dk");
        countryNameToCode.Add("Ecuador", "ec");
        countryNameToCode.Add("Egypt", "eg");
        countryNameToCode.Add("Estonia", "ee");
        countryNameToCode.Add("Finland", "fi");
        countryNameToCode.Add("France", "fr");
        countryNameToCode.Add("Georgia", "ge");
        countryNameToCode.Add("Germany", "de");
        countryNameToCode.Add("Ghana", "gh");
        countryNameToCode.Add("Greece", "gr");
        countryNameToCode.Add("Hungary", "hu");
        countryNameToCode.Add("Iceland", "is");
        countryNameToCode.Add("India", "in");
        countryNameToCode.Add("Indonesia", "id");
        countryNameToCode.Add("Iran", "ir");
        countryNameToCode.Add("Iraq", "iq");
        countryNameToCode.Add("Ireland", "ie");
        countryNameToCode.Add("Israel", "il");
        countryNameToCode.Add("Italy", "it");
        countryNameToCode.Add("Jamaica", "jm");
        countryNameToCode.Add("Japan", "jp");
        countryNameToCode.Add("Jordan", "jo");
        countryNameToCode.Add("Kazakhstan", "kz");
        countryNameToCode.Add("Kenya", "ke");
        countryNameToCode.Add("Kuwait", "kw");
        countryNameToCode.Add("Latvia", "lv");
        countryNameToCode.Add("Lebanon", "lb");
        countryNameToCode.Add("Lithuania", "lt");
        countryNameToCode.Add("Luxembourg", "lu");
        countryNameToCode.Add("Malaysia", "my");
        countryNameToCode.Add("Mexico", "mx");
        countryNameToCode.Add("Morocco", "ma");
        countryNameToCode.Add("Nepal", "np");
        countryNameToCode.Add("Netherlands", "nl");
        countryNameToCode.Add("New Zealand", "nz");
        countryNameToCode.Add("Nigeria", "ng");
        countryNameToCode.Add("Norway", "no");
        countryNameToCode.Add("Pakistan", "pk");
        countryNameToCode.Add("Peru", "pe");
        countryNameToCode.Add("Philippines", "ph");
        countryNameToCode.Add("Poland", "pl");
        countryNameToCode.Add("Portugal", "pt");
        countryNameToCode.Add("Qatar", "qa");
        countryNameToCode.Add("Romania", "ro");
        countryNameToCode.Add("Russia", "ru");
        countryNameToCode.Add("Saudi Arabia", "sa");
        countryNameToCode.Add("Singapore", "sg");
        countryNameToCode.Add("Slovakia", "sk");
        countryNameToCode.Add("Slovenia", "si");
        countryNameToCode.Add("South Africa", "za");
        countryNameToCode.Add("South Korea", "kr");
        countryNameToCode.Add("Spain", "es");
        countryNameToCode.Add("Sri Lanka", "lk");
        countryNameToCode.Add("Sweden", "se");
        countryNameToCode.Add("Switzerland", "ch");
        countryNameToCode.Add("Thailand", "th");
        countryNameToCode.Add("Turkey", "tr");
        countryNameToCode.Add("Ukraine", "ua");
        countryNameToCode.Add("United Arab Emirates", "ae");
        countryNameToCode.Add("United Kingdom", "gb");
        countryNameToCode.Add("United States", "us");
        countryNameToCode.Add("Uruguay", "uy");
        countryNameToCode.Add("Venezuela", "ve");
        countryNameToCode.Add("Vietnam", "vn");
        countryNameToCode.Add("Zimbabwe", "zw");

        // Common alternative names
        countryNameToCode.Add("USA", "us");
        countryNameToCode.Add("UK", "gb");
        countryNameToCode.Add("England", "gb");
        countryNameToCode.Add("Britain", "gb");
        countryNameToCode.Add("Korea", "kr");
        countryNameToCode.Add("Unknown", "");
    }

    void LoadFlagsFromResources()
    {
        if (string.IsNullOrEmpty(flagsResourcesPath)) return;

        // Load all sprites from Resources/Flags/ folder
        Sprite[] loadedFlags = Resources.LoadAll<Sprite>(flagsResourcesPath);

        if (showDebugLogs)
        {
            Debug.Log($"Loaded {loadedFlags.Length} flag sprites from Resources/{flagsResourcesPath}");
        }

        foreach (Sprite flagSprite in loadedFlags)
        {
            // Try to determine country from filename
            string fileName = flagSprite.name.ToLower();
            string countryCode = ExtractCountryCodeFromFilename(fileName);
            string countryName = GetCountryNameFromCode(countryCode);

            if (!string.IsNullOrEmpty(countryName))
            {
                flagDatabase.Add(new CountryFlagData(countryName, countryCode, flagSprite));

                if (showDebugLogs)
                {
                    Debug.Log($"Added flag: {flagSprite.name} -> {countryName} ({countryCode})");
                }
            }
        }
    }

    string ExtractCountryCodeFromFilename(string fileName)
    {
        // Handle common naming patterns:
        // flag_us.png, flag-us.png, us.png, flag_us_small.png, etc.

        // Remove common prefixes and suffixes
        fileName = fileName.Replace("flag_", "").Replace("flag-", "").Replace("_small", "").Replace("_large", "");

        // Look for 2-letter country codes
        foreach (var kvp in countryNameToCode)
        {
            if (fileName.Contains(kvp.Value) && kvp.Value.Length == 2)
            {
                return kvp.Value;
            }
        }

        // If filename is exactly 2 characters, assume it's a country code
        if (fileName.Length == 2)
        {
            return fileName;
        }

        return "";
    }

    string GetCountryNameFromCode(string countryCode)
    {
        foreach (var kvp in countryNameToCode)
        {
            if (kvp.Value == countryCode)
            {
                return kvp.Key;
            }
        }
        return "";
    }

    void BuildFlagDatabase()
    {
        // Build cache for quick lookups
        flagCache.Clear();

        foreach (var flagData in flagDatabase)
        {
            if (flagData.flagSprite != null)
            {
                string key = flagData.countryName.ToLower();
                if (!flagCache.ContainsKey(key))
                {
                    flagCache.Add(key, flagData.flagSprite);
                }

                // Also add by country code
                if (!string.IsNullOrEmpty(flagData.countryCode))
                {
                    string codeKey = flagData.countryCode.ToLower();
                    if (!flagCache.ContainsKey(codeKey))
                    {
                        flagCache.Add(codeKey, flagData.flagSprite);
                    }
                }
            }
        }
    }

    void PreloadCommonFlags()
    {
        foreach (string country in commonCountries)
        {
            GetCountryFlag(country); // This will cache it
        }

        if (showDebugLogs)
        {
            Debug.Log($"Preloaded {commonCountries.Length} common flags");
        }
    }

    public void SetCountryFlag(Image flagImage, string countryName)
    {
        if (flagImage == null) return;

        Sprite flagSprite = GetCountryFlag(countryName);
        flagImage.sprite = flagSprite;

        if (showDebugLogs && flagSprite == defaultFlag)
        {
            Debug.LogWarning($"Using default flag for country: {countryName}");
        }
    }

    public Sprite GetCountryFlag(string countryName)
    {
        if (string.IsNullOrEmpty(countryName)) return defaultFlag;

        string key = countryName.ToLower();

        // Check cache first
        if (flagCache.ContainsKey(key))
        {
            return flagCache[key];
        }

        // Try partial matching for alternative country names
        foreach (var kvp in flagCache)
        {
            if (kvp.Key.Contains(key) || key.Contains(kvp.Key))
            {
                // Cache this match for future use
                flagCache[key] = kvp.Value;
                return kvp.Value;
            }
        }

        // Try by country code
        if (countryNameToCode.ContainsKey(countryName))
        {
            string countryCode = countryNameToCode[countryName];
            if (flagCache.ContainsKey(countryCode))
            {
                flagCache[key] = flagCache[countryCode]; // Cache by name too
                return flagCache[countryCode];
            }
        }

        return defaultFlag;
    }

    public bool HasFlag(string countryName)
    {
        return GetCountryFlag(countryName) != defaultFlag;
    }

    public List<string> GetAvailableCountries()
    {
        List<string> countries = new List<string>();
        foreach (var flagData in flagDatabase)
        {
            countries.Add(flagData.countryName);
        }
        return countries;
    }

    // Method to manually add a flag at runtime
    public void AddFlag(string countryName, string countryCode, Sprite flagSprite)
    {
        if (flagSprite == null) return;

        flagDatabase.Add(new CountryFlagData(countryName, countryCode, flagSprite));

        // Update cache
        string key = countryName.ToLower();
        flagCache[key] = flagSprite;

        if (!string.IsNullOrEmpty(countryCode))
        {
            flagCache[countryCode.ToLower()] = flagSprite;
        }

        if (showDebugLogs)
        {
            Debug.Log($"Added flag for {countryName} ({countryCode})");
        }
    }

    // Editor helper to populate flags manually
#if UNITY_EDITOR
    [ContextMenu("Auto-Populate From Resources")]
    void EditorAutoPopulate()
    {
        flagDatabase.Clear();
        LoadFlagsFromResources();
        BuildFlagDatabase();
        Debug.Log($"Auto-populated {flagDatabase.Count} flags from Resources/{flagsResourcesPath}");
    }
#endif
}