using System;
using System.Collections.Generic;
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

        public static ResourceDictionary DynamoModernDictionary
        {
            get
            {
                if (_dynamoModernDictionary == null)
                {
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/DynamoModern.xaml",
                                        System.UriKind.Relative);

                    _dynamoModernDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/DataTemplates.xaml",
                                        System.UriKind.Relative);

                    _dataTemplatesDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/DynamoColorsAndBrushes.xaml",
                                        System.UriKind.Relative);

                    _dynamoColorsAndBrushesDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/DynamoConverters.xaml",
                                        System.UriKind.Relative);

                    _dynamoConvertersDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/DynamoText.xaml",
                                        System.UriKind.Relative);

                    _dynamoTextDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/MenuStyleDictionary.xaml",
                                        System.UriKind.Relative);

                    _menuStyleDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/ToolbarStyleDictionary.xaml",
                                        System.UriKind.Relative);

                    _toolbarStyleDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/Connectors.xaml",
                                        System.UriKind.Relative);

                    _connectorsDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
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
                    System.Uri resourceLocater =
                        new System.Uri("/DynamoCore;component/UI/Themes/Ports.xaml",
                                        System.UriKind.Relative);

                    _portsDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
                }

                return _portsDictionary;
            }
        }
    }
}
