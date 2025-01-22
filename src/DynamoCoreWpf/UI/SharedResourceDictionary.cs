using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Dynamo.UI
{
    /// <summary>
    /// The shared resource dictionary is a specialized resource dictionary
    /// that loads it content only once. If a second instance with the same source
    /// is created, it only merges the resources from the cache.
    /// </summary>
    public class SharedResourceDictionary : ResourceDictionary
    {
        /// <summary>
        /// Internal cache of loaded dictionaries 
        /// </summary>
        public static Dictionary<Uri, ResourceDictionary> _sharedDictionaries =
            new Dictionary<Uri, ResourceDictionary>();

        /// <summary>
        /// Local member of the source uri
        /// </summary>
        private Uri _sourceUri;

        /// <summary>
        /// Returns or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get => _sourceUri;
            set
            {
                if (_sourceUri == value) return;

                _sourceUri = value;

                try
                {
                    if (!_sharedDictionaries.ContainsKey(value))
                    {
                        // Load and cache the dictionary
                        base.Source = value;
                        _sharedDictionaries.Add(value, this);
                    }
                    else
                    {
                        // Use the cached dictionary
                        MergedDictionaries.Add(_sharedDictionaries[value]);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load ResourceDictionary from '{value}'.", ex);
                }
            }
        }

    }

    public static class SharedDictionaryManager
    {
        private static ResourceDictionary _dynamoModernDictionary;

        public static string ThemesDirectory 
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"UI\Themes\");
            }
        }

        //public static Uri DynamoModernDictionaryUri
        //{
        //    get {return new Uri(Path.Combine(ThemesDirectory, "Modern_Combined.xaml")); }
        //}

        public static Uri DynamoModernDictionaryUri
        {
            get => new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Themes/Modern_Combined.xaml");
        }

        public static ResourceDictionary DynamoModernDictionary
        {
            get {
                return _dynamoModernDictionary ??
                       (_dynamoModernDictionary = new ResourceDictionary() {Source = DynamoModernDictionaryUri});
            }
        }
    }
}
