using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.UI;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the theme resource resolution added by DYN-6228: which folder a dictionary is
    /// loaded from, the per-file fallback to the base theme, and the integrity of the light
    /// palette.
    /// </summary>
    /// <remarks>
    /// These tests mutate <see cref="SharedDictionaryManager.CurrentTheme"/>, which is global
    /// static state, and <see cref="SharedResourceDictionary"/> caches every dictionary it loads
    /// in a static dictionary that is never cleared. The theme is therefore always restored in
    /// TearDown, and dictionaries are loaded through plain <see cref="ResourceDictionary"/>
    /// instances rather than the cached SharedDictionaryManager properties, so that no other
    /// fixture in the same process inherits a light-theme brush.
    /// </remarks>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ThemeResourceTests
    {
        private const string BaseThemeFolder = @"UI\Themes\Modern";
        private const string LightThemeFolder = @"UI\Themes\Light";

        private DynamoTheme originalTheme;

        /// <summary>
        /// Every palette key looked up from C# with
        /// <c>SharedDictionaryManager.DynamoColorsAndBrushesDictionary["..."]</c>.
        /// A missing key yields a null brush and a TypeInitializationException at runtime rather
        /// than a build error, so this list is the guard against that.
        /// Add to it whenever a new lookup is introduced.
        /// </summary>
        private static readonly string[] KeysReadFromCode =
        {
            "Blue300Brush",
            "Blue350",
            "Blue400Brush",
            "Blue450",
            "ChevronHighlightOverlayBackground",
            "CodeEditorClassBrush",
            "CodeEditorLinkBrush",
            "CodeEditorMethodBrush",
            "CodeEditorNumberBrush",
            "CurveMapperCurveBrush",
            "CurveMapperGridLineBrush",
            // Looked up through a local variable or a helper rather than the dictionary directly
            // (ObjectTypeConverter, the library member converter, CodeBlockEditorUtils).
            "ActionMembersColor",
            "CreateMembersColor",
            "QueryMembersColor",
            "WatchTreeListLabelBrush",
            "boolLabelBackground",
            "nullLabelBackground",
            "numberLabelBackground",
            "objectLabelBackground",
            "stringLabelBackground",
            "DarkBlue200Brush",
            "DarkGreyBrush",
            "DarkMidGreyBrush",
            "DarkerGrey",
            "LightGreyBrush",
            "MidGreyBrush",
            "NodeBodyBackgroundBrush",
            "NodeContextMenuAccentBrush",
            "NodeContextMenuBackground",
            "NodeContextMenuBackgroundHighlight",
            "NodeContextMenuForeground",
            "NodeContextMenuForegroundHighlight",
            "NodeContextMenuSeparatorColor",
            "NodeDismissedWarningsGlyphBackground",
            "NodeDismissedWarningsGlyphForeground",
            "NodeHeaderBackgroundBrush",
            "NodeInputBackgroundBrush",
            "NodeInputBorderBrush",
            "NodeInputCaretBrush",
            "NodeInputForegroundBrush",
            "NodeInputHighlightBrush",
            "NodeOptionsButtonBackground",
            "NodeTransientOverlayColor",
            "PortBackgroundBrush",
            "PortBackgroundPreviewOffBrush",
            "PortBorderBrush",
            "PortKeepListStructureBackground",
            "PortKeepListStructureBorderBrush",
            "PortLabelForegroundBrush",
            "PortMouseOverColor",
            "PortValueMarkerBlueBrush",
            "PortValueMarkerDefaultBrush",
            "PortValueMarkerGreyBrush",
            "PortValueMarkerRedBrush",
            "PrimaryCharcoal100Brush",
            "PrimaryCharcoal200Brush",
            "PrimaryCharcoal300Brush",
            "Red500Brush",
            "UnSelectedLayoutForeground",
            "WhiteColor",
            "WorkspaceBackgroundCustom",
            "WorkspaceBackgroundHome",
            "WorkspaceBackgroundHomeBrush",
            "YellowOrange500Brush"
        };

        /// <summary>
        /// Keys whose whole purpose is to differ between the two themes. If one of these stops
        /// differing, a light-theme surface has silently reverted to its dark value.
        /// </summary>
        private static readonly string[] RoleKeysThatMustDiffer =
        {
            "WorkspaceBackgroundBrush",
            "NodeBodyBackgroundBrush",
            "NodeHeaderBackgroundBrush",
            "NodeExpanderBackground",
            "NodeExpanderHoverBackground",
            "PortLabelForegroundBrush",
            "PortBackgroundBrush",
            "NodeInputBackgroundBrush",
            "NodeInputForegroundBrush",
            "NodeContextMenuBackground",
            "NodeContextMenuForeground",
            "CodeEditorBackgroundBrush",
            "CodeEditorForegroundBrush",
            "InCanvasSearchBackgroundBrush",
            "InCanvasSearchForegroundBrush",
            "InCanvasSearchResultBackgroundBrush",
            "InCanvasSearchResultForegroundBrush",
            "InCanvasSearchTooltipBackgroundBrush",
            "PreviewBubbleBackgroundBrush",
            "PreviewBubbleForegroundBrush",
            "nullLabelBackground",
            "WatchTreeBackgroundBrush",
            "WatchTreeForegroundBrush",
            "BooleanControlForegroundBrush",
            "RunSettingsComboBackgroundBrush",
            "RunSettingsComboForegroundBrush",
            "RunSettingsComboItemDisabledBrush",
            "NodeDropDownBackgroundBrush",
            "NodeDropDownForegroundBrush",
            "NodeDropDownPopupBackgroundBrush",
            "NodeLabelForegroundBrush",
            "NodeSelectionTextBrush",
            "CurveMapperCurveBrush",
            "WatchTreeListLabelBrush",
            "WatchTreeAccentBrush",
            "AutoCompleteGlyphBrush",
            "AutoCompleteButtonHoverBrush",
            "AutoCompleteSearchForegroundBrush",
            "AutoCompleteItemHighlightBrush",
            "InCanvasSearchResultHoverBrush",
            "InCanvasSearchResultHighlightBrush",
            "InCanvasSearchTooltipTextBrush",
            "InCanvasSearchTooltipHeaderBrush",
            "NodeExpanderGlyphBrush"
        };

        /// <summary>
        /// Keys serving surfaces that stay dark in every theme. Overriding one of these in the
        /// light palette would leak a light colour onto the Package Manager or the Python editor.
        /// </summary>
        private static readonly string[] KeysThatMustNotBeThemed =
        {
            "DividerRectangleBrush",
            "MainMenuSeparatorBrush",
            "PythonEditorForegroundBrush",
            "PMContextMenuForeground",
            "PMContextMenuForegroundHighlight",
            "PMContextMenuBackground",
            "PMContextMenuBackgroundHighlight",
            "PMContextMenuSeparatorColor"
        };

        [SetUp]
        public void SetUp()
        {
            // Loading a loose ResourceDictionary by URI relies on WPF resource/pack-URI
            // machinery that only gets initialized once an Application exists. When this fixture
            // runs on its own (no other fixture has created a window first), that never happens,
            // and even the simplest dictionary fails to load. Force it here instead.
            if (Application.Current == null)
            {
                new Application();
            }

            originalTheme = SharedDictionaryManager.CurrentTheme;

            // SharedResourceDictionary caches every dictionary it loads by URI and never expires
            // that cache, so a dictionary loaded under one theme in an earlier test would otherwise
            // be handed back unchanged here, even though CurrentTheme has since switched.
            SharedResourceDictionary._sharedDictionaries.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SharedDictionaryManager.CurrentTheme = originalTheme;
            SharedResourceDictionary._sharedDictionaries.Clear();
        }

        private static ResourceDictionary LoadDictionary(Uri uri)
        {
            return new ResourceDictionary { Source = uri };
        }

        private static ResourceDictionary LoadPalette(DynamoTheme theme)
        {
            SharedDictionaryManager.CurrentTheme = theme;
            return LoadDictionary(SharedDictionaryManager.DynamoColorsAndBrushesDictionaryUri);
        }

        #region Folder resolution

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsDarkThenThemesDirectoryPointsToBaseThemeFolder()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Dark;

            StringAssert.EndsWith(BaseThemeFolder + Path.DirectorySeparatorChar,
                SharedDictionaryManager.ThemesDirectory);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsLightThenThemesDirectoryPointsToLightFolder()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Light;

            StringAssert.EndsWith(LightThemeFolder + Path.DirectorySeparatorChar,
                SharedDictionaryManager.ThemesDirectory);
        }

        /// <summary>
        /// The light theme ships its own palette, so the URI must resolve into the Light folder.
        /// If this fails, the most likely cause is that the csproj does not copy
        /// UI\Themes\Light\DynamoColorsAndBrushes.xaml to the output directory, in which case the
        /// light theme silently falls back to dark with no error at runtime.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenThemeOverridesDictionaryThenUriPointsToThemeFolder()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Light;

            var uri = SharedDictionaryManager.DynamoColorsAndBrushesDictionaryUri;

            StringAssert.Contains(LightThemeFolder, uri.LocalPath);
            Assert.IsTrue(File.Exists(uri.LocalPath),
                $"Light palette was not copied to the output directory: {uri.LocalPath}");
        }

        /// <summary>
        /// A theme only ships the dictionaries whose colours differ; everything else must resolve
        /// against the base theme.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenThemeDoesNotOverrideDictionaryThenUriFallsBackToBaseTheme()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Light;

            var uri = SharedDictionaryManager.PortsDictionaryUri;

            StringAssert.Contains(BaseThemeFolder, uri.LocalPath);
            Assert.IsTrue(File.Exists(uri.LocalPath));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBaseDictionaryUriIsRequestedThenItIgnoresCurrentTheme()
        {
            SharedDictionaryManager.CurrentTheme = DynamoTheme.Light;
            var underLight = SharedDictionaryManager.BaseDynamoColorsAndBrushesDictionaryUri;

            SharedDictionaryManager.CurrentTheme = DynamoTheme.Dark;
            var underDark = SharedDictionaryManager.BaseDynamoColorsAndBrushesDictionaryUri;

            Assert.AreEqual(underDark, underLight);
            StringAssert.Contains(BaseThemeFolder, underLight.LocalPath);
        }

        #endregion

        #region Palette integrity

        /// <summary>
        /// The light palette merges the base palette and redefines only what it changes. An own
        /// key that does not exist in the base palette is a typo that would otherwise go unnoticed
        /// until the surface using it rendered with the wrong colour.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenEveryOverrideExistsInBasePalette()
        {
            var basePalette = LoadPalette(DynamoTheme.Dark);
            var lightPalette = LoadPalette(DynamoTheme.Light);

            var orphans = lightPalette.Keys
                .Cast<object>()
                .Select(k => k.ToString())
                .Where(k => !basePalette.Contains(k))
                .ToList();

            Assert.IsEmpty(orphans,
                "Light palette defines keys that do not exist in the base palette: " +
                string.Join(", ", orphans));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenEveryKeyReadFromCodeResolvesInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                // The indexer traverses MergedDictionaries and returns null when the key is
                // absent; Contains() only looks at the dictionary's own keys, so it would report
                // every inherited key as missing from the light palette.
                var unresolved = KeysReadFromCode.Where(k => palette[k] == null).ToList();

                Assert.IsEmpty(unresolved,
                    $"[{theme}] palette does not resolve keys that C# looks up, which would " +
                    $"produce a null brush at runtime: {string.Join(", ", unresolved)}");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenRoleKeysDifferFromBasePalette()
        {
            var basePalette = LoadPalette(DynamoTheme.Dark);
            var lightPalette = LoadPalette(DynamoTheme.Light);

            var unchanged = new List<string>();
            foreach (var key in RoleKeysThatMustDiffer)
            {
                Assert.IsTrue(basePalette.Contains(key), $"Base palette has no key '{key}'.");
                Assert.IsTrue(lightPalette.Contains(key), $"Light palette has no key '{key}'.");

                if (ColorOf(basePalette[key]) == ColorOf(lightPalette[key]))
                {
                    unchanged.Add(key);
                }
            }

            Assert.IsEmpty(unchanged,
                "These keys are supposed to be re-themed but resolve to the same colour in both " +
                "themes: " + string.Join(", ", unchanged));
        }

        /// <summary>
        /// Package Manager, the Python editor and the main menu bar stay dark in every theme.
        /// They were given dedicated keys precisely so that re-theming the canvas menus could not
        /// leak into them.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenDarkOnlyKeysAreNotOverridden()
        {
            LoadPalette(DynamoTheme.Dark);
            var lightPalette = LoadPalette(DynamoTheme.Light);

            var overridden = KeysThatMustNotBeThemed
                .Where(k => lightPalette.Keys.Cast<object>().Any(o => o.ToString() == k))
                .ToList();

            Assert.IsEmpty(overridden,
                "The light palette overrides keys that must stay dark: " +
                string.Join(", ", overridden));
        }

        /// <summary>
        /// Port labels sit directly on the node body in both themes, so if these two ever resolve
        /// to a similar colour the port names become unreadable. This is the defect the light
        /// theme originally shipped with.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenPortLabelContrastsWithNodeBodyInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var body = ColorOf(palette["NodeBodyBackgroundBrush"]);
                var label = ColorOf(palette["PortLabelForegroundBrush"]);

                var ratio = ContrastRatio(body, label);
                Assert.GreaterOrEqual(ratio, 4.5d,
                    $"[{theme}] port label {label} on node body {body} has a contrast ratio of " +
                    $"{ratio:F2}:1, below the 4.5:1 required for readable text.");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenNodeInputTextContrastsWithItsFieldInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var field = ColorOf(palette["NodeInputBackgroundBrush"]);
                var text = ColorOf(palette["NodeInputForegroundBrush"]);

                var ratio = ContrastRatio(field, text);
                Assert.GreaterOrEqual(ratio, 4.5d,
                    $"[{theme}] node input text {text} on field {field} has a contrast ratio of " +
                    $"{ratio:F2}:1, below the 4.5:1 required for readable text.");
            }
        }

        /// <summary>
        /// The in-canvas search box is embedded in the canvas and group context menus, so its
        /// input row has to follow the canvas theme rather than the library sidebar it borrows
        /// its result templates from.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenInCanvasSearchTextContrastsWithItsBoxInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var box = ColorOf(palette["InCanvasSearchBackgroundBrush"]);

                foreach (var key in new[]
                {
                    "InCanvasSearchForegroundBrush",
                    "InCanvasSearchPlaceholderBrush"
                })
                {
                    var text = ColorOf(palette[key]);
                    var ratio = ContrastRatio(box, text);

                    Assert.GreaterOrEqual(ratio, 4.5d,
                        $"[{theme}] {key} {text} on the search box {box} has a contrast ratio " +
                        $"of {ratio:F2}:1, below the 4.5:1 required for readable text.");
                }
            }
        }

        /// <summary>
        /// A disabled run type (for example Periodic, when the graph cannot run periodically) is
        /// still listed in the dropdown and must be legible, while reading as dimmer than an
        /// enabled one. Only the light theme is asserted: the dark theme has always drawn it as
        /// translucent white (about 1.9:1), which is pre-existing and out of scope. WCAG exempts
        /// inactive controls from 4.5:1, so 3:1 is the bar here.
        /// </summary>
        /// <summary>
        /// Node dropdowns (Custom Selection and every other DSDropDownBase node, Curve Mapper, the
        /// converter and unit nodes) share RefreshComboBox. Its text must read both in the closed
        /// box and in the open list, which use different backgrounds.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenNodeDropDownTextContrastsWithBoxAndListInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);
                var text = ColorOf(palette["NodeDropDownForegroundBrush"]);

                foreach (var surface in new[]
                {
                    "NodeDropDownBackgroundBrush",
                    "NodeDropDownPopupBackgroundBrush",
                    "NodeDropDownHighlightBrush"
                })
                {
                    var background = ColorOf(palette[surface]);
                    var ratio = ContrastRatio(background, text);

                    // The light theme is held to 4.5:1. The dark theme keeps its pre-existing
                    // values, which are below that and out of scope for DYN-6228: #DCDCDC on
                    // #666666 is 4.19:1 and on the #808080 highlight is 2.88:1. Its bars stop a
                    // regression without failing on the historical palette.
                    double required;
                    if (theme == DynamoTheme.Light)
                    {
                        required = 4.5d;
                    }
                    else
                    {
                        required = surface == "NodeDropDownHighlightBrush" ? 2.5d : 4.0d;
                    }

                    Assert.GreaterOrEqual(ratio, required,
                        $"[{theme}] dropdown text {text} on {surface} {background} is {ratio:F2}:1.");
                }
            }
        }

        /// <summary>
        /// Labels drawn directly on the node body by Define Data, Element Selection and Curve Mapper
        /// previously borrowed light-only colours, which vanished on the light node body.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenNodeBodyLabelsContrastWithTheNodeBodyInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);
                var body = ColorOf(palette["NodeBodyBackgroundBrush"]);

                foreach (var key in new[]
                {
                    "NodeLabelForegroundBrush",
                    "NodeSelectionTextBrush",
                    "CurveMapperLabelForegroundBrush"
                })
                {
                    var label = ColorOf(palette[key]);
                    var ratio = ContrastRatio(body, label);

                    Assert.GreaterOrEqual(ratio, 4.5d,
                        $"[{theme}] {key} {label} on the node body {body} is {ratio:F2}:1.");
                }
            }
        }

        /// <summary>
        /// Everything drawn as text in the watch tree (Watch node and preview bubble): the "List"
        /// label and value colours returned by ObjectTypeConverter, the item count, and the index
        /// of a leaf item, which sits on a chip that ListIndexBackgroundConverter hard-codes.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenWatchTreeTextIsReadableInBothThemes()
        {
            // ListIndexBackgroundConverter returns this literal for leaf items in both themes.
            var leafIndexChip = (Color)ColorConverter.ConvertFromString("#DCDCDC");

            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);
                var tree = ColorOf(palette["WatchTreeBackgroundBrush"]);

                foreach (var key in new[]
                {
                    "WatchTreeListLabelBrush",
                    "WatchTreeAccentBrush",
                    "objectLabelBackground",
                    "numberLabelBackground",
                    "stringLabelBackground",
                    "boolLabelBackground",
                    "nullLabelBackground"
                })
                {
                    var text = ColorOf(palette[key]);
                    var ratio = ContrastRatio(tree, text);
                    Assert.GreaterOrEqual(ratio, 4.5d,
                        $"[{theme}] watch tree text {key} {text} on {tree} is {ratio:F2}:1.");
                }

                var index = ColorOf(palette["WatchTreeIndexForegroundBrush"]);
                var indexRatio = ContrastRatio(leafIndexChip, index);
                Assert.GreaterOrEqual(indexRatio, 4.5d,
                    $"[{theme}] leaf index {index} on its {leafIndexChip} chip is {indexRatio:F2}:1.");
            }
        }

        /// <summary>
        /// The value-type colours double as node label-chip backgrounds under white text, so
        /// darkening them for the watch tree must not break the chips.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenValueTypeChipsKeepWhiteTextReadable()
        {
            var palette = LoadPalette(DynamoTheme.Light);

            foreach (var key in new[]
            {
                "objectLabelBackground",
                "numberLabelBackground",
                "stringLabelBackground",
                "boolLabelBackground",
                "nullLabelBackground"
            })
            {
                var chip = ColorOf(palette[key]);
                var ratio = ContrastRatio(chip, Colors.White);
                Assert.GreaterOrEqual(ratio, 4.5d, $"White text on {key} {chip} is {ratio:F2}:1.");
            }
        }

        /// <summary>
        /// The node autocomplete bar draws its Accept, Cancel and navigation glyphs from white
        /// PNGs recoloured through an opacity mask, so they must contrast with the bar and with
        /// the button hover in both themes. Also covers the search text and the highlighted
        /// dropdown item, which previously borrowed dark Package Manager colours.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenAutoCompleteBarIsReadableInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                // Search text in the dark theme has always been #C0C0C0 on #535353 (4.23:1),
                // which is pre-existing; the light theme is held to 4.5:1.
                var searchRequired = theme == DynamoTheme.Light ? 4.5d : 4.0d;

                var checks = new[]
                {
                    ("AutoCompleteGlyphBrush", "autocompletionWindow", 4.5d),
                    ("AutoCompleteGlyphBrush", "AutoCompleteButtonHoverBrush", 4.5d),
                    ("AutoCompleteSearchForegroundBrush", "autocompletionWindow", searchRequired),
                    ("AutocompletionWindowFontColor", "AutoCompleteItemHighlightBrush", 4.5d)
                };

                foreach (var (foregroundKey, backgroundKey, required) in checks)
                {
                    var foreground = ColorOf(palette[foregroundKey]);
                    var background = ColorOf(palette[backgroundKey]);
                    var ratio = ContrastRatio(background, foreground);

                    Assert.GreaterOrEqual(ratio, required,
                        $"[{theme}] {foregroundKey} {foreground} on {backgroundKey} {background} " +
                        $"is {ratio:F2}:1.");
                }
            }
        }

        /// <summary>
        /// The Python script editor stays dark in every theme. Its plain text previously borrowed
        /// a key the light theme darkens, which left identifiers dark grey on the dark editor.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenPythonEditorTextContrastsWithTheDarkEditorInBothThemes()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var editor = ColorOf(palette["TextEditorBrush"]);
                var text = ColorOf(palette["PythonEditorForegroundBrush"]);
                var ratio = ContrastRatio(editor, text);

                Assert.GreaterOrEqual(ratio, 4.5d,
                    $"[{theme}] Python editor text {text} on {editor} is {ratio:F2}:1.");
            }
        }

        /// <summary>
        /// Right-click search results and the node tooltip beside them follow the canvas theme.
        /// Only the light theme is asserted for the secondary colours: several dark values
        /// (for example #808285 parameters on the #404040 hover, 2.69:1) are pre-existing.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenInCanvasSearchResultsAndTooltipAreReadable()
        {
            var palette = LoadPalette(DynamoTheme.Light);

            var checks = new[]
            {
                ("InCanvasSearchResultForegroundBrush", "InCanvasSearchResultBackgroundBrush", 4.5d),
                ("InCanvasSearchResultForegroundBrush", "InCanvasSearchResultHoverBrush", 4.5d),
                ("InCanvasSearchResultForegroundBrush", "InCanvasSearchResultHighlightBrush", 4.5d),
                ("InCanvasSearchResultSecondaryForegroundBrush", "InCanvasSearchResultBackgroundBrush", 4.5d),
                ("InCanvasSearchResultSecondaryForegroundBrush", "InCanvasSearchResultHoverBrush", 4.5d),
                ("InCanvasSearchTooltipTextBrush", "InCanvasSearchTooltipBackgroundBrush", 4.5d),
                ("InCanvasSearchTooltipMutedBrush", "InCanvasSearchTooltipBackgroundBrush", 4.5d),
                ("InCanvasSearchTooltipHeaderBrush", "InCanvasSearchTooltipBackgroundBrush", 4.5d),
                ("InCanvasSearchTooltipAccentBrush", "InCanvasSearchTooltipBackgroundBrush", 4.5d),
                // "No description available" is deliberately muted.
                ("InCanvasSearchTooltipDescriptionMissingBrush", "InCanvasSearchTooltipBackgroundBrush", 3.0d)
            };

            foreach (var (foregroundKey, backgroundKey, required) in checks)
            {
                var foreground = ColorOf(palette[foregroundKey]);
                var background = ColorOf(palette[backgroundKey]);
                var ratio = ContrastRatio(background, foreground);

                Assert.GreaterOrEqual(ratio, required,
                    $"{foregroundKey} {foreground} on {backgroundKey} {background} is {ratio:F2}:1.");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLightPaletteIsLoadedThenDisabledRunTypeItemIsReadableButDimmer()
        {
            var palette = LoadPalette(DynamoTheme.Light);

            var dropdown = ColorOf(palette["RunSettingsComboBackgroundBrush"]);
            var disabled = ColorOf(palette["RunSettingsComboItemDisabledBrush"]);
            var enabled = ColorOf(palette["DynamoStandardLabelTextBrush"]);

            Assert.AreEqual(255, disabled.A,
                "The disabled item colour must be opaque in the light theme; a translucent " +
                "white is invisible on the light dropdown.");

            var disabledRatio = ContrastRatio(dropdown, disabled);
            var enabledRatio = ContrastRatio(dropdown, enabled);

            Assert.GreaterOrEqual(disabledRatio, 3.0d,
                $"Disabled run type text {disabled} on {dropdown} is {disabledRatio:F2}:1.");
            Assert.Less(disabledRatio, enabledRatio,
                "A disabled run type should read as dimmer than an enabled one.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenContextMenuRestingTextContrastsWithItsBackground()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var background = ColorOf(palette["NodeContextMenuBackground"]);
                var foreground = ColorOf(palette["NodeContextMenuForeground"]);

                var ratio = ContrastRatio(background, foreground);
                Assert.GreaterOrEqual(ratio, 4.5d,
                    $"[{theme}] context menu text {foreground} on {background} has a contrast " +
                    $"ratio of {ratio:F2}:1, below the 4.5:1 required for readable text.");
            }
        }

        /// <summary>
        /// The hovered state is held to the 3:1 threshold rather than 4.5:1 because the dark
        /// theme has always drawn white on #808080, which is 3.95:1. That is pre-existing
        /// behaviour and out of scope for DYN-6228; the check exists to stop a theme dropping
        /// below it, which is what happened when the light theme first shipped white-on-#DCD8D2.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPaletteIsLoadedThenContextMenuHoverTextContrastsWithItsBackground()
        {
            foreach (var theme in new[] { DynamoTheme.Dark, DynamoTheme.Light })
            {
                var palette = LoadPalette(theme);

                var background = ColorOf(palette["NodeContextMenuBackgroundHighlight"]);
                var foreground = ColorOf(palette["NodeContextMenuForegroundHighlight"]);

                var ratio = ContrastRatio(background, foreground);
                Assert.GreaterOrEqual(ratio, 3.0d,
                    $"[{theme}] hovered context menu text {foreground} on {background} has a " +
                    $"contrast ratio of {ratio:F2}:1.");
            }
        }

        #endregion

        #region Helpers

        private static Color ColorOf(object resource)
        {
            if (resource is SolidColorBrush brush) return brush.Color;
            if (resource is Color color) return color;

            throw new AssertionException(
                $"Expected a SolidColorBrush or Color but found {resource?.GetType().Name ?? "null"}.");
        }

        /// <summary>
        /// WCAG 2.1 relative luminance contrast ratio between two opaque colours.
        /// </summary>
        private static double ContrastRatio(Color a, Color b)
        {
            var la = RelativeLuminance(a);
            var lb = RelativeLuminance(b);
            var lighter = Math.Max(la, lb);
            var darker = Math.Min(la, lb);

            return (lighter + 0.05d) / (darker + 0.05d);
        }

        private static double RelativeLuminance(Color c)
        {
            return 0.2126d * LinearChannel(c.R)
                 + 0.7152d * LinearChannel(c.G)
                 + 0.0722d * LinearChannel(c.B);
        }

        private static double LinearChannel(byte channel)
        {
            var v = channel / 255d;
            return v <= 0.03928d ? v / 12.92d : Math.Pow((v + 0.055d) / 1.055d, 2.4d);
        }

        #endregion
    }
}
