using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

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
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"UI\Themes\Modern\");
            }
        }

        public static ResourceDictionary DynamoModernDictionary
        {
            get
            {
                if (_dynamoModernDictionary == null)
                {
                    var resourceLocater =new Uri(Path.Combine(ThemesDirectory, "DynamoModern.xaml"));
                    _dynamoModernDictionary = new ResourceDictionary(){Source = resourceLocater};
                }

                return _dynamoModernDictionary;
            }
        }

        public static ResourceDictionary DataTemplatesDictionary
        {
            get
            {
                if (_dataTemplatesDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "DataTemplates.xaml"));
                    _dataTemplatesDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _dataTemplatesDictionary;
            }
        }

        public static ResourceDictionary DynamoColorsAndBrushesDictionary
        {
            get
            {
                if (_dynamoColorsAndBrushesDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "DynamoColorsAndBrushes.xaml"));
                    _dynamoColorsAndBrushesDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _dynamoColorsAndBrushesDictionary;
            }
        }

        public static ResourceDictionary DynamoConvertersDictionary
        {
            get
            {
                if (_dynamoConvertersDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "DynamoConverters.xaml"));
                    _dynamoConvertersDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _dynamoConvertersDictionary;
            }
        }

        public static ResourceDictionary DynamoTextDictionary
        {
            get
            {
                if (_dynamoTextDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "DynamoText.xaml"));
                    _dynamoTextDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _dynamoTextDictionary;
            }
        }

        public static ResourceDictionary MenuStyleDictionary
        {
            get
            {
                if (_menuStyleDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "MenuStyleDictionary.xaml"));
                    _menuStyleDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _menuStyleDictionary;
            }
        }

        public static ResourceDictionary ToolbarStyleDictionary
        {
            get
            {
                if (_toolbarStyleDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "ToolbarStyleDictionary.xaml"));
                    _toolbarStyleDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _toolbarStyleDictionary;
            }
        }

        public static ResourceDictionary ConnectorsDictionary
        {
            get
            {
                if (_connectorsDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "Connectors.xaml"));
                    _connectorsDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _connectorsDictionary;
            }
        }

        public static ResourceDictionary PortsDictionary
        {
            get
            {
                if (_portsDictionary == null)
                {
                    var resourceLocater = new Uri(Path.Combine(ThemesDirectory, "Ports.xaml"));
                    _portsDictionary = new ResourceDictionary() { Source = resourceLocater };
                }

                return _portsDictionary;
            }
        }
    }
}
