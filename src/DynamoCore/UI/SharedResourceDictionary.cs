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
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get { return _sourceUri; }
            set
            {
                _sourceUri = value;

                if (!_sharedDictionaries.ContainsKey(value))
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class
                    base.Source = value;

                    // add it to the cache
                    _sharedDictionaries.Add(value, this);
                }
                else
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(_sharedDictionaries[value]);
                }
            }
        }
    }

    public static class SharedDictionaryManager
    {
        private static ResourceDictionary _dynamoModernDictionary;
        private static ResourceDictionary _dataTemplatesDictionary;
        private static ResourceDictionary _dynamoColorsAndBrushesDictionary;
        private static ResourceDictionary _dynamoConvertersDictionary;
        private static ResourceDictionary _dynamoTextDictionary;
        private static ResourceDictionary _menuStyleDictionary;
        private static ResourceDictionary _toolbarStyleDictionary;
        private static ResourceDictionary _connectorsDictionary;
        private static ResourceDictionary _portsDictionary;

        public static string ThemesDirectory 
        {
            get
            {
                return Path.Combine( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"UI\Themes\Modern\");
            }
        }

        public static Uri DynamoModernDictionaryUri
        {
            get {return new Uri(Path.Combine(ThemesDirectory, "DynamoModern.xaml")); }
        }

        public static Uri DataTemplatesDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "DataTemplates.xaml")); }
        }

        public static Uri DynamoColorsAndBrushesDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "DynamoColorsAndBrushes.xaml")); }
        }

        public static Uri DynamoConvertersDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "DynamoConverters.xaml")); }
        }

        public static Uri DynamoTextDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "DynamoText.xaml")); }
        }

        public static Uri MenuStyleDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "MenuStyleDictionary.xaml")); }
        }

        public static Uri ToolbarStyleDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "ToolbarStyleDictionary.xaml")); }
        }

        public static Uri ConnectorsDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "Connectors.xaml")); }
        }

        public static Uri PortsDictionaryUri
        {
            get { return new Uri(Path.Combine(ThemesDirectory, "Ports.xaml")); }
        }

        public static ResourceDictionary DynamoModernDictionary
        {
            get {
                return _dynamoModernDictionary ??
                       (_dynamoModernDictionary = new ResourceDictionary() {Source = DynamoModernDictionaryUri});
            }
        }

        public static ResourceDictionary DataTemplatesDictionary
        {
            get {
                return _dataTemplatesDictionary ??
                       (_dataTemplatesDictionary = new ResourceDictionary() {Source = DataTemplatesDictionaryUri});
            }
        }

        public static ResourceDictionary DynamoColorsAndBrushesDictionary
        {
            get {
                return _dynamoColorsAndBrushesDictionary ??
                       (_dynamoColorsAndBrushesDictionary = new ResourceDictionary() { Source = DynamoColorsAndBrushesDictionaryUri });
            }
        }

        public static ResourceDictionary DynamoConvertersDictionary
        {
            get {
                return _dynamoConvertersDictionary ??
                       (_dynamoConvertersDictionary = new ResourceDictionary() {Source = DynamoConvertersDictionaryUri});
            }
        }

        public static ResourceDictionary DynamoTextDictionary
        {
            get {
                return _dynamoTextDictionary ??
                       (_dynamoTextDictionary = new ResourceDictionary() {Source = DynamoTextDictionaryUri});
            }
        }

        public static ResourceDictionary MenuStyleDictionary
        {
            get {
                return _menuStyleDictionary ??
                       (_menuStyleDictionary = new ResourceDictionary() {Source = MenuStyleDictionaryUri});
            }
        }

        public static ResourceDictionary ToolbarStyleDictionary
        {
            get {
                return _toolbarStyleDictionary ??
                       (_toolbarStyleDictionary = new ResourceDictionary() { Source = ToolbarStyleDictionaryUri });
            }
        }

        public static ResourceDictionary ConnectorsDictionary
        {
            get {
                return _connectorsDictionary ??
                       (_connectorsDictionary = new ResourceDictionary() {Source = ConnectorsDictionaryUri});
            }
        }

        public static ResourceDictionary PortsDictionary
        {
            get {
                return _portsDictionary ?? (_portsDictionary = new ResourceDictionary() {Source = PortsDictionaryUri});
            }
        }
    }
}
