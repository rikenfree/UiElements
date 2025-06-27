using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// This class is responsible for managing the color palette and tokens for the UI.
/// Export the color palette and tokens from Figma using the plugin "Design Tokens Manager"
/// and place the .token files in the Resources folder.
/// </summary>
public static class ColorManager
{
    public const string HIGH_ALPHA = "<alpha=#FF>";
    public const string MID_ALPHA = "<alpha=#99>";
    public const string LOW_ALPHA = "<alpha=#61>";

    private static Dictionary<string, object> colorPalette;
    private static Dictionary<string, object> colorTokens;

    private static bool isInitialized = false;

    private const string PALETTE_PATH = "Colour Values.Mode 1.tokens";
    private const string TOKENS_PATH = "Colour Tokens.Enabled.tokens";
    private const string DEFAULT_COLOR =
#if UNITY_EDITOR
        // Magenta for debugging in the editor
        "#FF00FFFF";
#else
        // White for production
        "#FFFFFFFF";
#endif

    [Serializable]
    public class ColorDefinition
    {
        [JsonProperty("$type")]
        public string Type;

        [JsonProperty("$value")]
        public string Value;
    }

    static ColorManager()
    {
        Initialize();
    }

    /// <summary>
    /// Get a Color by token, e.g. "system/text/light/high"
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static Color GetColorByToken(string token)
    {
        return HexToColor(GetHexByToken(token));
    }

    /// <summary>
    /// Get a hex string by token, e.g. "system/text/light/high" => "#FFFFFFFF"
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static string GetHexByToken(string token)
    {
        // Ensure the manager is initialized before attempting to fetch a color
        if (!isInitialized)
        {
            Initialize();

            // Initialization failed
            if (!isInitialized) return DEFAULT_COLOR;
        }

        // Replace all spaces with underscores to match the token format
        token = token.Replace(" ", "_");

        // Split the token into keys to get the palette reference
        string[] tokenParts = token.Trim('{', '}').Split('/');

        // Try to resolve the keys
        return ResolveTokenParts(tokenParts);
    }

    /// <summary>
    /// Resolve a token by trying to traverse the 2 dictionaries.
    /// </summary>
    /// <param name="tokenParts"></param>
    /// <returns></returns>
    private static string ResolveTokenParts(string[] tokenParts)
    {
        // If the value is a hex color, return it directly
        if (IsHexColor(tokenParts[0])) return tokenParts[0];

        // Check if it is a token
        var tokenDef = TraverseDictionary(colorTokens, tokenParts) as ColorDefinition;
        if (tokenDef != null)
        {
            // A result was found, split the result into keys and resovle it again
            return ResolveTokenParts(tokenDef.Value.Trim('{', '}').Split('.'));
        }

        // It is not a token, try to resolve it as a palette reference
        var colorDef = TraverseDictionary(colorPalette, tokenParts) as ColorDefinition;
        if (colorDef != null)
        {
            // A result was found, split the result into keys and resovle it again
            return ResolveTokenParts(colorDef.Value.Trim('{', '}').Split('.'));
        }

        ColorLogger.LogError($"Token not found: <b>{string.Join("/", tokenParts)}</b>");
        return DEFAULT_COLOR;
    }

    /// <summary>
    /// Convert a hex color string to a Unity Color.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color HexToColor(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1); // Remove the '#' character

        // Handle RGBA
        if (hex.Length == 8)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, a);
        }
        // Handle RGB (if applicable)
        else if (hex.Length == 6)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255); // Assume fully opaque if no alpha
        }

        ColorLogger.LogError("Invalid hex format: " + hex);
        return Color.white;
    }

    /// <summary>
    /// Initialize the ColorManager by loading the color palette and tokens from JSON files.
    /// </summary>
    private static void Initialize()
    {
        if (isInitialized) return; // Only initialize once

        try
        {
            // Load the JSON files from the Resources folder
            TextAsset paletteTextAsset = Resources.Load<TextAsset>(PALETTE_PATH);
            TextAsset tokenTextAsset = Resources.Load<TextAsset>(TOKENS_PATH);

            if (paletteTextAsset == null)
            {
                throw new FileNotFoundException($"Color palette file not found! Expected location: Resources/{PALETTE_PATH}");
            }

            if (tokenTextAsset == null)
            {
                throw new FileNotFoundException($"Color tokens file not found! Expected location: Resources/{TOKENS_PATH}");
            }

            // Deserialize the JSON files into dictionaries
            colorPalette = JsonConvert.DeserializeObject<Dictionary<string, object>>(paletteTextAsset.text, new JsonDictionaryConverter());
            colorTokens = JsonConvert.DeserializeObject<Dictionary<string, object>>(tokenTextAsset.text, new JsonDictionaryConverter());

            // Log some debug stats about the loaded data
            // Iterate over the color palette and tokens and count the number of entries
            void CountEntries(Dictionary<string, object> dict, out int count)
            {
                count = 0;
                foreach (var entry in dict)
                {
                    if (entry.Value is Dictionary<string, object> subDict)
                    {
                        CountEntries(subDict, out int subCount);
                        count += subCount;
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            CountEntries(colorPalette, out int paletteCount);
            CountEntries(colorTokens, out int tokenCount);
            ColorLogger.Log($"Color palette loaded with <b>{paletteCount}</b> entries and <b>{tokenCount}</b> tokens.");

            isInitialized = true;
        }
        catch (Exception e)
        {
            ColorLogger.LogError($"Error initializing ColorManager: {e.Message}");
        }
    }

    /// <summary>
    /// Traverse a nested dictionary using an array of keys.
    /// Handles an arbitrary depth of nested dictionaries.
    /// </summary>
    /// <param name="dictionary">The dictionary to traverse.</param>
    /// <param name="keys">The keys to use for traversal.</param>
    /// <returns>The value found at the end of the traversal, or null if not found.</returns>
    private static object TraverseDictionary(Dictionary<string, object> dictionary, string[] keys, int index = 0)
    {
        //ColorLogger.Log($"Traversal: {string.Join("/", keys)} at index {index}");

        // Invalid traversal, out of bounds
        if (index >= keys.Length)
        {
            ColorLogger.LogError("<b>Traversal out of bounds!</b>");
            return null;
        }

        // Key not found in dictionary
        if (!dictionary.TryGetValue(keys[index], out var next))
        {
#if DEBUG_COLOR
                ColorLogger.LogWarning($"Key <b>{keys[index]}</b> not found for {string.Join("/", keys)}\n"
                    + $"Available keys: {string.Join(", ", dictionary.Keys)}");
#endif
            return null;
        }

        // Final key, return the value
        if (index == keys.Length - 1) return next;

        // If next is a dictionary, continue traversing recursively
        if (next is Dictionary<string, object> nextDict)
        {
            return TraverseDictionary(nextDict, keys, index + 1);
        }

        // Invalid type, cannot traverse further
        ColorLogger.LogError($"Invalid type for key <b>{keys[index]}</b> in {string.Join("/", keys)}");
        return null;
    }

    /// <summary>
    /// Check if a string is a valid hex color.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsHexColor(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;

        // Check if the string starts with '#' and has a length of 7 (RGB) or 9 (RGBA)
        if (value.StartsWith("#") && (value.Length == 7 || value.Length == 9))
        {
            // Check if the remaining characters are valid hex digits
            for (int i = 1; i < value.Length; i++)
            {
                if (!Uri.IsHexDigit(value[i]))
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }
}

/// <summary>
/// Custom JSON converter for deserializing nested dictionaries for <see cref="ColorManager"/>.
/// </summary>
public class JsonDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<string, object>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return ReadValue(JToken.Load(reader));
    }

    private object ReadValue(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                // Check if this is a leaf node that should be converted to ColorDefinition
                if (IsColorDefinition(token))
                {
                    // Deserialize this node as a ColorDefinition
                    return token.ToObject<ColorManager.ColorDefinition>();
                }
                else
                {
                    // Otherwise, it's an intermediate node; continue to create a dictionary
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in token.Children<JProperty>())
                    {
                        dict[prop.Name] = ReadValue(prop.Value);
                    }
                    return dict;
                }

            case JTokenType.Array:
                var list = new List<object>();
                foreach (var item in token.Children())
                {
                    list.Add(ReadValue(item));
                }
                return list;

            default:
                return ((JValue)token).Value;
        }
    }

    /// <summary>
    /// Check if a JToken is a <see cref="ColorManager.ColorDefinition">
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool IsColorDefinition(JToken token)
    {
        return token.Type == JTokenType.Object &&
            token["$type"] != null &&
            token["$value"] != null &&
            token["$type"].Type == JTokenType.String &&
            token["$value"].Type == JTokenType.String &&
            token["$type"].Value<string>() == "color";
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public static class ColorLogger
{
    public static void Log(string message)
    {
#if DEBUG_COLOR
            Debug.Log($"<color=#13BC90>[ColorManager]</color> {message}\n");
#endif
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning($"<color=#13BC90>[ColorManager]</color> {message}\n");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"<color=#13BC90>[ColorManager]</color> {message}\n");
    }
}