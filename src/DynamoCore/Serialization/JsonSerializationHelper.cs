using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dynamo.Serialization
{
    /// <summary>
    /// Helper class for JSON serialization using System.Text.Json.
    /// Provides utilities to replace Newtonsoft.Json functionality.
    /// </summary>
    public static class JsonSerializationHelper
    {
        /// <summary>
        /// Creates default JsonSerializerOptions for Dynamo serialization.
        /// </summary>
        /// <param name="converters">Optional custom converters to include</param>
        /// <returns>Configured JsonSerializerOptions</returns>
        public static JsonSerializerOptions CreateSerializerOptions(params JsonConverter[] converters)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = false,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                // Use invariant culture for consistent serialization
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Add custom converters
            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    options.Converters.Add(converter);
                }
            }

            return options;
        }

        /// <summary>
        /// Creates JsonSerializerOptions for deserialization with backward compatibility.
        /// </summary>
        /// <param name="converters">Optional custom converters to include</param>
        /// <returns>Configured JsonSerializerOptions</returns>
        public static JsonSerializerOptions CreateDeserializerOptions(params JsonConverter[] converters)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true, // More lenient for reading old files
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            // Add custom converters
            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    options.Converters.Add(converter);
                }
            }

            return options;
        }

        /// <summary>
        /// Safely gets a string value from a JsonElement.
        /// </summary>
        public static string GetStringOrDefault(JsonElement element, string defaultValue = "")
        {
            return element.ValueKind == JsonValueKind.String ? element.GetString() ?? defaultValue : defaultValue;
        }

        /// <summary>
        /// Safely gets an int value from a JsonElement.
        /// </summary>
        public static int GetInt32OrDefault(JsonElement element, int defaultValue = 0)
        {
            return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Safely gets a double value from a JsonElement.
        /// </summary>
        public static double GetDoubleOrDefault(JsonElement element, double defaultValue = 0.0)
        {
            return element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Safely gets a bool value from a JsonElement.
        /// </summary>
        public static bool GetBooleanOrDefault(JsonElement element, bool defaultValue = false)
        {
            if (element.ValueKind == JsonValueKind.True) return true;
            if (element.ValueKind == JsonValueKind.False) return false;
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a Guid value from a JsonElement.
        /// </summary>
        public static Guid GetGuidOrDefault(JsonElement element, Guid defaultValue = default)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (Guid.TryParse(str, out var guid))
                {
                    return guid;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Tries to get a property from a JsonElement.
        /// </summary>
        public static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                return element.TryGetProperty(propertyName, out property);
            }
            property = default;
            return false;
        }

        /// <summary>
        /// Gets an array of JsonElements from a property, or empty array if not found.
        /// </summary>
        public static JsonElement[] GetArrayOrEmpty(JsonElement element, string propertyName)
        {
            if (TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                var list = new List<JsonElement>();
                foreach (var item in property.EnumerateArray())
                {
                    list.Add(item);
                }
                return list.ToArray();
            }
            return Array.Empty<JsonElement>();
        }

        /// <summary>
        /// Deserializes a JsonElement to a specific type using the provided options.
        /// </summary>
        public static T Deserialize<T>(JsonElement element, JsonSerializerOptions options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Parses a JSON string and returns a JsonDocument.
        /// The caller is responsible for disposing the returned JsonDocument.
        /// </summary>
        public static JsonDocument ParseJson(string json)
        {
            return JsonDocument.Parse(json);
        }

        /// <summary>
        /// Writes a JSON value with error handling.
        /// </summary>
        public static void WriteValue(Utf8JsonWriter writer, string propertyName, string value)
        {
            writer.WriteString(propertyName, value);
        }

        /// <summary>
        /// Writes a JSON value with error handling.
        /// </summary>
        public static void WriteValue(Utf8JsonWriter writer, string propertyName, int value)
        {
            writer.WriteNumber(propertyName, value);
        }

        /// <summary>
        /// Writes a JSON value with error handling.
        /// </summary>
        public static void WriteValue(Utf8JsonWriter writer, string propertyName, double value)
        {
            writer.WriteNumber(propertyName, value);
        }

        /// <summary>
        /// Writes a JSON value with error handling.
        /// </summary>
        public static void WriteValue(Utf8JsonWriter writer, string propertyName, bool value)
        {
            writer.WriteBoolean(propertyName, value);
        }

        /// <summary>
        /// Writes a JSON value with error handling.
        /// </summary>
        public static void WriteValue(Utf8JsonWriter writer, string propertyName, Guid value)
        {
            writer.WriteString(propertyName, value.ToString());
        }
    }
}
