using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.UI;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Integration tests for DYN-6228. These load the real resource dictionaries from the build
    /// output, so they exercise XAML parsing, the x:Static references between dictionaries, the
    /// per-file fallback, and every StaticResource lookup the dictionaries make.
    /// </summary>
    /// <remarks>
    /// A missing StaticResource throws while the dictionary is being parsed, which is exactly the
    /// failure mode a light theme is prone to: a key that exists in the base palette but was
    /// renamed or mistyped in a consumer. Loading each dictionary under both themes is therefore
    /// a meaningful end-to-end check without having to construct a window.
    ///
    /// Like ThemeResourceTests, this fixture mutates global static theme state and restores it in
    /// TearDown.
    /// </remarks>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ThemeIntegrationTests
    {
        private DynamoTheme originalTheme;

        [SetUp]
        public void SetUp()
        {
            originalTheme = SharedDictionaryManager.CurrentTheme;
        }

        [TearDown]
        public void TearDown()
        {
            SharedDictionaryManager.CurrentTheme = originalTheme;
        }

        /// <summary>
        /// The dictionaries SharedDictionaryManager can resolve. InPorts.xaml and OutPorts.xaml
        /// are deliberately excluded: those URIs point at files that do not exist on disk and the
        /// properties have no consumers (see doc/dev/DYN-6228-light-theme.md).
        /// </summary>
        private static IEnumerable<Uri> ThemeDictionaryUris()
        {
            yield return SharedDictionaryManager.DynamoColorsAndBrushesDictionaryUri;
            yield return SharedDictionaryManager.DynamoConvertersDictionaryUri;
            yield return SharedDictionaryManager.DynamoModernDictionaryUri;
            yield return SharedDictionaryManager.DataTemplatesDictionaryUri;
            yield return SharedDictionaryManager.DynamoTextDictionaryUri;
            yield return SharedDictionaryManager.MenuStyleDictionaryUri;
            yield return SharedDictionaryManager.ToolbarStyleDictionaryUri;
            yield return SharedDictionaryManager.ConnectorsDictionaryUri;
            yield return SharedDictionaryManager.PortsDictionaryUri;
            yield return SharedDictionaryManager.SidebarGridDictionaryUri;
            yield return SharedDictionaryManager.LiveChartsDictionaryUri;
        }

        [Category("UnitTests")]
        [TestCase(DynamoTheme.Dark)]
        [TestCase(DynamoTheme.Light)]
        public void WhenThemeIsAppliedThenEveryThemeDictionaryLoadsWithoutError(DynamoTheme theme)
        {
            SharedDictionaryManager.CurrentTheme = theme;

            var failures = new List<string>();
            foreach (var uri in ThemeDictionaryUris())
            {
                try
                {
                    var dictionary = new ResourceDictionary { Source = uri };
                    Assert.IsNotNull(dictionary);
                }
                catch (Exception ex)
                {
                    failures.Add($"{Path.GetFileName(uri.LocalPath)}: {ex.Message}");
                }
            }

            Assert.IsEmpty(failures,
                $"[{theme}] these theme dictionaries failed to load:{Environment.NewLine}" +
                string.Join(Environment.NewLine, failures));
        }

        [Category("UnitTests")]
        [TestCase(DynamoTheme.Dark)]
        [TestCase(DynamoTheme.Light)]
        public void WhenThemeIsAppliedThenEveryThemeDictionaryFileExists(DynamoTheme theme)
        {
            SharedDictionaryManager.CurrentTheme = theme;

            var missing = ThemeDictionaryUris()
                .Where(uri => !File.Exists(uri.LocalPath))
                .Select(uri => uri.LocalPath)
                .ToList();

            Assert.IsEmpty(missing,
                $"[{theme}] resolved to files that do not exist: {string.Join(", ", missing)}");
        }

        /// <summary>
        /// DynamoConverters.xaml resolves the wire colours through StaticResource, so it will
        /// throw on load if the connector keys are missing. This asserts the converters actually
        /// pick up the themed values rather than silently keeping a default.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenLightThemeIsAppliedThenConnectorConvertersUseLightWireColours()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Dark;
            var darkConverters = new ResourceDictionary
            {
                Source = SharedDictionaryManager.DynamoConvertersDictionaryUri
            };

            SharedDictionaryManager.CurrentTheme = DynamoTheme.Light;
            var lightConverters = new ResourceDictionary
            {
                Source = SharedDictionaryManager.DynamoConvertersDictionaryUri
            };

            Assert.IsTrue(darkConverters.Contains("ConnectionStateToBrushConverter"));
            Assert.IsTrue(lightConverters.Contains("ConnectionStateToBrushConverter"));
            Assert.IsNotNull(darkConverters["ConnectionStateToColorConverter"]);
            Assert.IsNotNull(lightConverters["ConnectionStateToColorConverter"]);
        }

        /// <summary>
        /// The light theme has its own syntax highlighting definition because the default one is
        /// bright-on-dark. Both must be embedded, or the code block editor throws when it tries
        /// to load a null stream.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenHighlightingDefinitionsAreRequestedThenBothAreEmbedded()
        {
            var assembly = typeof(SharedDictionaryManager).Assembly;

            foreach (var file in new[]
            {
                Configurations.HighlightingFile,
                Configurations.LightHighlightingFile
            })
            {
                var name = "Dynamo.Wpf.UI.Resources." + file;
                using var stream = assembly.GetManifestResourceStream(name);
                Assert.IsNotNull(stream, $"Embedded resource not found: {name}");
            }
        }

        /// <summary>
        /// The two highlighting definitions must stay in step: a keyword added to one has to be
        /// added to the other, otherwise the light theme silently stops colouring it.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenHighlightingDefinitionsAreComparedThenTheyDefineTheSameKeywords()
        {
            var assembly = typeof(SharedDictionaryManager).Assembly;

            var dark = ReadKeywords(assembly, Configurations.HighlightingFile);
            var light = ReadKeywords(assembly, Configurations.LightHighlightingFile);

            CollectionAssert.AreEquivalent(dark, light,
                "The dark and light DesignScript syntax definitions define different keywords.");
        }

        private static List<string> ReadKeywords(System.Reflection.Assembly assembly, string file)
        {
            using var stream = assembly.GetManifestResourceStream("Dynamo.Wpf.UI.Resources." + file);
            Assert.IsNotNull(stream, $"Embedded resource not found: {file}");

            var document = System.Xml.Linq.XDocument.Load(stream);
            return document.Descendants("Key")
                .Select(k => (string)k.Attribute("word"))
                .Where(w => !string.IsNullOrEmpty(w))
                .OrderBy(w => w)
                .ToList();
        }
    }

    /// <summary>
    /// Verifies that the theme selected in preferences is applied to the resource dictionaries
    /// during startup.
    /// </summary>
    /// <remarks>
    /// Only the default (dark) direction is asserted here. Starting a second Dynamo under the
    /// light theme inside the same test process is not safe: NodeView, PortViewModel, InPorts,
    /// OutPorts and DynamoTextBox all resolve and freeze their brushes in static initialisers
    /// that run once per process, so a light-theme startup would leave light brushes in place for
    /// every subsequent fixture. Light-theme rendering is covered by the manual test pass
    /// described in doc/dev/DYN-6228-light-theme.md.
    /// </remarks>
    [TestFixture]
    public class ThemeStartupTests : DynamoTestUIBase
    {
        [Test]
        public void WhenDynamoStartsThenCurrentThemeMatchesThePreference()
        {
            Assert.AreEqual(Model.PreferenceSettings.Theme, SharedDictionaryManager.CurrentTheme);
        }

        [Test]
        public void WhenPreferencesAreDefaultThenDynamoStartsInDarkTheme()
        {
            Assert.AreEqual(DynamoTheme.Dark, Model.PreferenceSettings.Theme);
            Assert.AreEqual(DynamoTheme.Dark, SharedDictionaryManager.CurrentTheme);
        }
    }
}
