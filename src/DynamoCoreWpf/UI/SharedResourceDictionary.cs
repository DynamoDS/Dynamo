using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Dynamo.Configuration;

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
        private static ResourceDictionary _sidebarGridDictionary;
        private static ResourceDictionary outPortsDictionary;
        private static ResourceDictionary inPortsDictionary;
        private static ResourceDictionary _liveChartDictionary;

        /// <summary>
        /// Folder name of the theme that ships the complete set of resource dictionaries.
        /// Every other theme folder only needs to contain the files it overrides.
        /// </summary>
        private const string BaseThemeFolderName = "Modern";

        private static DynamoTheme currentTheme = DynamoTheme.Dark;
        private static bool isThemeLatched;

        /// <summary>
        /// The theme whose resource dictionaries are loaded.
        /// </summary>
        /// <remarks>
        /// The theme is fixed for the lifetime of the process by <see cref="ApplyStartupTheme"/>.
        /// <see cref="SharedResourceDictionary"/> caches every dictionary by URI, WPF resolves most
        /// colors through <c>StaticResource</c>, and several views hold brushes in static fields,
        /// all of which survive a Dynamo session closing. Changing the theme later in the same
        /// process would therefore leave a mix of both themes.
        /// </remarks>
        public static DynamoTheme CurrentTheme
        {
            get { return currentTheme; }
            // Internal so that nothing outside Dynamo can bypass the once-per-process latch.
            // Tests assign it directly to load a given palette.
            internal set { currentTheme = value; }
        }

        /// <summary>
        /// Applies the theme for this process. Only the first call takes effect.
        /// </summary>
        /// <remarks>
        /// When Dynamo runs inside a host such as Revit, closing and reopening Dynamo starts a new
        /// session in the same process, and every static cache above still holds the theme the
        /// process started with. Latching keeps later sessions on that theme instead of mixing
        /// two, so a theme change needs the host application to restart as well.
        /// </remarks>
        /// <param name="requestedTheme">The theme stored in the user's preferences.</param>
        /// <returns>
        /// True if <paramref name="requestedTheme"/> is the theme in effect; false if an earlier
        /// session in this process already fixed a different theme.
        /// </returns>
        internal static bool ApplyStartupTheme(DynamoTheme requestedTheme)
        {
            if (!isThemeLatched)
            {
                currentTheme = requestedTheme;
                isThemeLatched = true;
            }

            return currentTheme == requestedTheme;
        }

        /// <summary>
        /// Whether <see cref="ApplyStartupTheme"/> has fixed the theme for this process.
        /// </summary>
        internal static bool IsThemeLatched
        {
            get { return isThemeLatched; }
        }

        /// <summary>
        /// Releases the latch so the next <see cref="ApplyStartupTheme"/> call takes effect.
        /// For tests only: in a real process the static caches would keep the old theme.
        /// </summary>
        internal static void ResetThemeLatchForTesting()
        {
            isThemeLatched = false;
        }

        /// <summary>
        /// Maps a theme to the folder under <c>UI\Themes\</c> that holds its dictionaries.
        /// </summary>
        private static string GetThemeFolderName(DynamoTheme theme)
        {
            // The dark theme keeps its historical folder name so that the ~66 XAML files
            // merging these dictionaries, and any package referencing them, keep working.
            return theme == DynamoTheme.Dark ? BaseThemeFolderName : theme.ToString();
        }

        /// <summary>
        /// Root folder holding all theme folders, e.g. <c>...\UI\Themes\</c>.
        /// </summary>
        private static string ThemesRootDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"UI\Themes\");
            }
        }

        /// <summary>
        /// Directory the resource dictionaries of the <see cref="CurrentTheme"/> are loaded from.
        /// </summary>
        public static string ThemesDirectory
        {
            get
            {
                return Path.Combine(ThemesRootDirectory, GetThemeFolderName(CurrentTheme)) +
                    Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Resolves a dictionary file against the current theme, falling back to the base theme
        /// when the current theme does not override that particular file.
        /// </summary>
        /// <param name="fileName">File name of the resource dictionary, e.g. "Ports.xaml".</param>
        /// <returns>An absolute URI to the dictionary that should be loaded.</returns>
        private static Uri GetThemedDictionaryUri(string fileName)
        {
            var themedPath = Path.Combine(ThemesDirectory, fileName);

            // Per-file fallback lets a theme ship only the dictionaries whose colors differ,
            // instead of duplicating all 13 files and drifting from the base theme over time.
            if (!File.Exists(themedPath))
            {
                var basePath = Path.Combine(ThemesRootDirectory, BaseThemeFolderName, fileName);
                if (File.Exists(basePath))
                {
                    return new Uri(basePath);
                }
            }

            return new Uri(themedPath);
        }

        public static Uri DynamoModernDictionaryUri
        {
            get { return GetThemedDictionaryUri("DynamoModern.xaml"); }
        }

        public static Uri DataTemplatesDictionaryUri
        {
            get { return GetThemedDictionaryUri("DataTemplates.xaml"); }
        }

        public static Uri DynamoColorsAndBrushesDictionaryUri
        {
            get { return GetThemedDictionaryUri("DynamoColorsAndBrushes.xaml"); }
        }

        /// <summary>
        /// URI of the palette belonging to the base theme, ignoring <see cref="CurrentTheme"/>.
        /// </summary>
        /// <remarks>
        /// A theme dictionary merges this and then redefines only the keys it changes, so themes
        /// stay small and automatically pick up keys added to the base palette.
        /// </remarks>
        public static Uri BaseDynamoColorsAndBrushesDictionaryUri
        {
            get
            {
                return new Uri(Path.Combine(ThemesRootDirectory, BaseThemeFolderName,
                    "DynamoColorsAndBrushes.xaml"));
            }
        }

        public static Uri DynamoConvertersDictionaryUri
        {
            get { return GetThemedDictionaryUri("DynamoConverters.xaml"); }
        }

        public static Uri DynamoTextDictionaryUri
        {
            get { return GetThemedDictionaryUri("DynamoText.xaml"); }
        }

        public static Uri MenuStyleDictionaryUri
        {
            get { return GetThemedDictionaryUri("MenuStyleDictionary.xaml"); }
        }

        public static Uri ToolbarStyleDictionaryUri
        {
            get { return GetThemedDictionaryUri("ToolbarStyleDictionary.xaml"); }
        }

        public static Uri ConnectorsDictionaryUri
        {
            get { return GetThemedDictionaryUri("Connectors.xaml"); }
        }

        public static Uri PortsDictionaryUri
        {
            get { return GetThemedDictionaryUri("Ports.xaml"); }
        }

        public static Uri OutPortsDictionaryUri
        {
            get { return GetThemedDictionaryUri("OutPorts.xaml"); }
        }

        public static Uri InPortsDictionaryUri
        {
            get { return GetThemedDictionaryUri("InPorts.xaml"); }
        }

        public static Uri SidebarGridDictionaryUri
        {
            get { return GetThemedDictionaryUri("SidebarGridStyleDictionary.xaml"); }
        }

        public static Uri LiveChartsDictionaryUri
        {
            get { return GetThemedDictionaryUri("LiveChartsStyle.xaml"); }
        }

        public static ResourceDictionary LiveChartDictionary
        {
            get
            {
                return _liveChartDictionary ??
                       (_liveChartDictionary = new ResourceDictionary() { Source = LiveChartsDictionaryUri });
            }
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

        public static ResourceDictionary OutPortsDictionary
        {
            get
            {
                return outPortsDictionary ?? (outPortsDictionary = new ResourceDictionary() { Source = OutPortsDictionaryUri });
            }
        }

        public static ResourceDictionary InPortsDictionary
        {
            get
            {
                return inPortsDictionary ?? (inPortsDictionary = new ResourceDictionary() { Source = InPortsDictionaryUri });
            }
        }

        public static ResourceDictionary SidebarGrid
        {
            get
            {
                return _sidebarGridDictionary ?? (_sidebarGridDictionary = new ResourceDictionary() { Source = SidebarGridDictionaryUri });
            }
        }
    }
}
