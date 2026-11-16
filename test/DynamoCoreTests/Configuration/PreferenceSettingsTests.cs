using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PreferenceSettingsTests : UnitTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestLoad()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            var settings = PreferenceSettings.Load(settingsFilePath);

            Assert.IsNotNull(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetPythonTemplateFilePath()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            Assert.IsTrue(File.Exists(initialPyFilePath));

            PreferenceSettings.Load(settingsFilePath);

            string pythonTemplateFilePath = Path.Combine(settingDirectory, PreferenceSettings.GetPythonTemplateFilePath());

            Assert.AreEqual(pythonTemplateFilePath, initialPyFilePath);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSettingsSerialization()
        {
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            PreferenceSettings settings = new PreferenceSettings();

            // Assert defaults
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), true);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, true);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.IsAutoSyncDocumentBrowser, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.BackupInterval, PreferenceSettings.DefaultBackupInterval);
            Assert.AreEqual(settings.UseHardwareAcceleration, true);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);
            Assert.AreEqual(settings.DynamoPlayerFolderGroups.Count, 0);
            Assert.AreEqual(settings.EnableDynamoPlayerRenamedWatchAsOutput, false);
            Assert.AreEqual(settings.Locale, "Default");

            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same when saved with defaults
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), true);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, true);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.IsAutoSyncDocumentBrowser, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.BackupInterval, PreferenceSettings.DefaultBackupInterval);
            Assert.AreEqual(settings.UseHardwareAcceleration, true);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);
            Assert.AreEqual(settings.DynamoPlayerFolderGroups.Count, 0);
            Assert.AreEqual(settings.EnableDynamoPlayerRenamedWatchAsOutput, false);
            Assert.AreEqual(settings.Locale, "Default");

            // Change setting values
            settings.SetIsBackgroundPreviewActive("MyBackgroundPreview", false);
            settings.ShowCodeBlockLineNumber = false;
            settings.ShowTabsAndSpacesInScriptEditor = true;
            settings.DefaultPythonEngine = "CP3";
            settings.MaxNumRecentFiles = 24;
            settings.BackupInterval = 120000; //change to 2 minutes(120000 ms)
            settings.UseHardwareAcceleration = false;
            settings.EnableNodeAutoComplete = false;
            settings.IsAutoSyncDocumentBrowser = false;
            settings.EnableNotificationCenter = false;
            settings.DefaultRunType = RunType.Manual;
            settings.ViewExtensionSettings.Add(new ViewExtensionSettings()
            {
                Name = "MyExtension",
                UniqueId = "1234",
                DisplayMode = ViewExtensionDisplayMode.FloatingWindow,
                WindowSettings = new WindowSettings()
                {
                    Left = 123,
                    Top = 456,
                    Height = 321,
                    Width = 654,
                    Status = WindowStatus.Maximized
                }
            });
            settings.GroupStyleItemsList.Add(new GroupStyleItem
            {
                Name = "TestGroup",
                HexColorString = "000000"
            });
            settings.DynamoPlayerFolderGroups.Add(new DynamoPlayerFolderGroup()
            {
                EntryPoint = "GenerativeDesign",
                Folders = new List<DynamoPlayerFolder>()
                {
                    new DynamoPlayerFolder()
                    {
                        Path = @"C:\MyGenerativeDesignFolder",
                        DisplayName = "My Generative Design Folder",
                        Id = "41B5B0F7-1B21-42A8-A938-E2C34521EF61",
                        IsRemovable = true,
                        Order = -1,
                    }
                }
            });
            settings.EnableDynamoPlayerRenamedWatchAsOutput = true;
            settings.Locale = "zh-CN";


            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same as last changed
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), false);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, false);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, true);
            Assert.AreEqual(settings.DefaultPythonEngine, "CP3");
            Assert.AreEqual(settings.MaxNumRecentFiles, 24);
            Assert.AreEqual(settings.BackupInterval, 120000);
            Assert.AreEqual(settings.UseHardwareAcceleration, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, false);
            Assert.AreEqual(settings.IsAutoSyncDocumentBrowser, false);
            Assert.AreEqual(settings.EnableNotificationCenter, false);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 1);
            var extensionSettings = settings.ViewExtensionSettings[0];
            Assert.AreEqual(settings.DefaultRunType, RunType.Manual);
            Assert.AreEqual(extensionSettings.Name, "MyExtension");
            Assert.AreEqual(extensionSettings.UniqueId, "1234");
            Assert.AreEqual(extensionSettings.DisplayMode, ViewExtensionDisplayMode.FloatingWindow);
            Assert.IsNotNull(extensionSettings.WindowSettings);
            var windowSettings = extensionSettings.WindowSettings;
            Assert.AreEqual(windowSettings.Left, 123);
            Assert.AreEqual(windowSettings.Top, 456);
            Assert.AreEqual(windowSettings.Height, 321);
            Assert.AreEqual(windowSettings.Width, 654);
            Assert.AreEqual(windowSettings.Status, WindowStatus.Maximized);
            // Load function will only deserialize the customized style
            Assert.AreEqual(settings.GroupStyleItemsList.Count, 1);
            var styleItemsList = settings.GroupStyleItemsList[0];
            Assert.AreEqual(styleItemsList.Name, "TestGroup");
            Assert.AreEqual(styleItemsList.HexColorString, "000000");
            Assert.AreEqual(settings.DynamoPlayerFolderGroups.Count, 1);
            Assert.AreEqual(settings.DynamoPlayerFolderGroups[0].Folders.Count, 1);
            Assert.AreEqual(settings.EnableDynamoPlayerRenamedWatchAsOutput, true);
            Assert.AreEqual(settings.Locale, "zh-CN");
        }

        [Test]
        [Category("UnitTests")]
        public void TestMigrateStdLibTokenToBuiltInToken()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-stdlibtoken.xml");
            Assert.IsTrue(File.ReadAllText(settingsFilePath).Contains(DynamoModel.StandardLibraryToken));
            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            var settings = PreferenceSettings.Load(settingsFilePath);

            var token = settings.CustomPackageFolders[1];

            Assert.AreEqual(DynamoModel.BuiltInPackagesToken, token);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSerializationDisableTrustWarnings()
        {
            //create new prefs
            var prefs = new PreferenceSettings();
            //assert default.
            Assert.IsFalse(prefs.DisableTrustWarnings);
            prefs.SetTrustWarningsDisabled(true);
            Assert.True(prefs.DisableTrustWarnings);
            //save
            var tempPath = GetNewFileNameOnTempPath(".xml");
            prefs.Save(tempPath);

            //load
            var settingsLoaded = PreferenceSettings.Load(tempPath);
            Assert.IsTrue(settingsLoaded.DisableTrustWarnings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSerializationTrustedLocations()
        {
            //create new prefs
            var prefs = new PreferenceSettings();
            //assert default.
            Assert.AreEqual(0, prefs.TrustedLocations.Count);
            prefs.SetTrustedLocations(new List<string>() { Path.GetTempPath() });
            Assert.AreEqual(1, prefs.TrustedLocations.Count);
            //save
            var tempPath = GetNewFileNameOnTempPath(".xml");
            prefs.Save(tempPath);

            //load
            var settingsLoaded = PreferenceSettings.Load(tempPath);
            Assert.AreEqual(1, settingsLoaded.TrustedLocations.Count);

            Assert.IsTrue(settingsLoaded.IsTrustedLocation(Path.GetTempPath()));
        }

        /// <summary>
        /// Struct to support the comparison between two PreferenceSettings instances
        /// </summary>
        struct PreferencesComparison
        {
            public List<string> Properties { get; set; }
            public List<String> SamePropertyValues { get; set; }
            public List<String> DifferentPropertyValues { get; set; }
        }

        /// <summary>
        /// Indicates if the Property has at least one custom attribute that is going to be excluded from the mapping
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        bool PropertyHasExcludedAttributes(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Length > 0 ||
                    property.GetCustomAttributes(typeof(System.Xml.Serialization.XmlIgnoreAttribute), true).Length > 0;
        }

        /// <summary>
        /// Checks if a property has a static mapped field
        /// </summary>
        /// <param name="preferenceInstance"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        bool PropertyHasStaticField(PreferenceSettings preferenceInstance, PropertyInfo property)
        {
            return preferenceInstance.StaticFields().ConvertAll(fieldName => fieldName.ToUpper()).Contains(property.Name.ToUpper());
        }

        /// <summary>
        /// Compare the property values of two PreferenceSettings instances
        /// </summary>
        /// <param name="defaultSettings"></param>
        /// <param name="newGeneralSettings"></param>
        /// <returns>3 List of Properties, the properties that have been evaluated, the properties that have the same values and the properties that have different values</returns>
        PreferencesComparison comparePrefenceSettings(PreferenceSettings defaultSettings, PreferenceSettings newGeneralSettings)
        {
            var result = new PreferencesComparison();
            var propertiesWithSameValue = new List<string>();
            var propertiesWithDifferentValue = new List<string>();
            var evaluatedProperties = new List<string>();
            var destinationProperties = defaultSettings.GetType().GetProperties();

            foreach (var destinationPi in destinationProperties)
            {
                var sourcePi = newGeneralSettings.GetType().GetProperty(destinationPi.Name);

                if (sourcePi.Name == "DynamoPlayerFolderGroups")
                {
                    // Do nothing for now
                }

                if (!PropertyHasExcludedAttributes(destinationPi) && !PropertyHasStaticField(defaultSettings, destinationPi))
                {
                    evaluatedProperties.Add(destinationPi.Name);
                    var newValue = sourcePi.GetValue(newGeneralSettings, null);
                    var oldValue = destinationPi.GetValue(defaultSettings, null);

                    if (destinationPi.PropertyType == typeof(List<string>))
                    {
                        var newList = (List<string>)sourcePi.GetValue(newGeneralSettings, null);
                        var oldList = (List<string>)destinationPi.GetValue(defaultSettings, null);
                        if (newList.Except(oldList).ToList().Count == 0)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<GroupStyleItem>))
                    {
                        if (((List<GroupStyleItem>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<GroupStyleItem>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<ViewExtensionSettings>))
                    {
                        if (((List<ViewExtensionSettings>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<ViewExtensionSettings>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<BackgroundPreviewActiveState>))
                    {
                        if (((List<BackgroundPreviewActiveState>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<BackgroundPreviewActiveState>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<DynamoPlayerFolderGroup>))
                    {
                        if (((List<DynamoPlayerFolderGroup>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<DynamoPlayerFolderGroup>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else
                    {
                        if (newValue?.ToString() == oldValue?.ToString())
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                }
            }

            result.SamePropertyValues = propertiesWithSameValue;
            result.DifferentPropertyValues = propertiesWithDifferentValue;
            result.Properties = evaluatedProperties;
            return result;
        }

        [Test]
        [Category("UnitTests")]
        public void TestImportCopySettings()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string newSettingslFilePath = Path.Combine(settingDirectory, "DynamoSettings-NewSettings.xml");

            var defaultSettings = new PreferenceSettings();
            var newSettings = PreferenceSettings.Load(newSettingslFilePath);

            // validation
            Assert.IsTrue(newSettings.IsCreatedFromValidFile, "The new settings file is invalid");

            bool newSettingsExist = File.Exists(newSettingslFilePath);
            var checkDifference = comparePrefenceSettings(defaultSettings, newSettings);
            int diffProps = checkDifference.DifferentPropertyValues.Count;
            int totProps = checkDifference.Properties.Count;
            string firstPropertyWithSameOrNewValue = checkDifference.Properties.Except(checkDifference.DifferentPropertyValues).ToList().FirstOrDefault();
            string defSettNumberFormat = defaultSettings.NumberFormat;
            string newSettNumberFormat = newSettings.NumberFormat;
            string failMessage = $"The file {newSettingslFilePath} exist: {newSettingsExist.ToString()} " +
                                 $"| DiffProps: {diffProps.ToString()} " +
                                 $"| TotProps: {totProps.ToString()} | Default Sett NumberFormat: {defSettNumberFormat} " +
                                 $"| New Sett NumberFormat: {newSettNumberFormat} " +
                                 $"| First Property with the same value {firstPropertyWithSameOrNewValue}";

            // checking if the new Setting are completely different from the Default
            Assert.IsTrue(checkDifference.DifferentPropertyValues.Count == checkDifference.Properties.Count, failMessage);

            // GroupStyle - Assigning Default styles
            defaultSettings.GroupStyleItemsList = GroupStyleItem.DefaultGroupStyleItems.AddRange(defaultSettings.GroupStyleItemsList.Where(style => style.IsDefault != true)).ToList();
            newSettings.CopyProperties(defaultSettings);
            // Checking if the new settings has at least a Custom Style
            Assert.IsTrue(defaultSettings.GroupStyleItemsList.Where(style => style.IsDefault == false).Count() > 0);

            // Explicit copy
            defaultSettings.SetTrustWarningsDisabled(newSettings.DisableTrustWarnings);
            defaultSettings.SetTrustedLocations(newSettings.TrustedLocations);

            // checking if the default Setting instance has the same property values of the new one
            var checkEquality = comparePrefenceSettings(defaultSettings, newSettings);
            Assert.IsTrue(checkEquality.SamePropertyValues.Count == checkEquality.Properties.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void TestTaintedFile()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string newSettingslFilePath = Path.Combine(settingDirectory, "DynamoSettings-TaintedSettings.xml");

            var newSettings = PreferenceSettings.Load(newSettingslFilePath);

            Assert.IsFalse(newSettings.IsCreatedFromValidFile, "The new settings file is valid");
        }

        [Test]
        [Category("UnitTests")]
        public void TestAskForTrustedLocation()
        {
            //Settings
            bool isOpenedFile = true;
            bool isHomeSpace = true;
            bool isShowStartPage = false;
            bool isFileInTrustedLocation = false;
            bool isDisableTrustWarnings = false;

            // getting result
            PreferenceSettings.AskForTrustedLocationResult result = PreferenceSettings.AskForTrustedLocation(
                isOpenedFile,
                isFileInTrustedLocation,
                isHomeSpace,
                isShowStartPage,
                isDisableTrustWarnings);

            // checking the result
            Assert.IsTrue(result == PreferenceSettings.AskForTrustedLocationResult.Ask, $"Conditions info: is opened file : {isOpenedFile} | is file in trusted location : {isFileInTrustedLocation} | is home space : {isHomeSpace} | is show Start page : {isShowStartPage} | is disable trust warnings : {isDisableTrustWarnings}");
        }

        [Test]
        [Category("UnitTests")]
        public void TestSanitizeValues()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingslFilePath = Path.Combine(settingDirectory, "DynamoSettings_Tainted_Values.xml");

            var settings = PreferenceSettings.Load(settingslFilePath);
            settings.SanitizeValues();

            bool allTheGroupStylesHaveAValidFontSize = true;
            foreach (var groupStyle in settings.GroupStyleItemsList)
            {
                if (!settings.PredefinedGroupStyleFontSizes.Contains(groupStyle.FontSize))
                {
                    allTheGroupStylesHaveAValidFontSize = false;
                    break;
                }
            }

            Assert.IsTrue(allTheGroupStylesHaveAValidFontSize, $"All the GroupStyles have a valid Font size : {allTheGroupStylesHaveAValidFontSize}");
        }

        [Test]
        [Category("UnitTests")]
        public void TestSerializingHomePageSettings()
        {
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "homePagePreference.xml");

            PreferenceSettings settings = new PreferenceSettings();

            // Assert defaults
            Assert.IsEmpty(settings.HomePageSettings);

            settings.HomePageSettings = new List<string> { { String.Concat("greeting", "Hello World") } };

            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            Assert.IsTrue(settings.HomePageSettings.Contains(String.Concat("greeting", "Hello World")));
        }

        [Test]
        [Category("UnitTests")]
        public void TestPathManagerSamplesDirectoryRespectsLocalePreference()
        {
            // Arrange
            var pathManagerParams = new PathManagerParams
            {
                CorePath = Path.GetDirectoryName(typeof(PathManager).Assembly.Location)
            };

            var pathManager = new PathManager(pathManagerParams);
            var preferences = new PreferenceSettings { Locale = "es-ES" };
            pathManager.Preferences = preferences;

            // Act
            var samplesPath = pathManager.SamplesDirectory;

            // Assert - samples path should contain the locale
            Assert.That(samplesPath, Is.Not.Null);
            var expectedPathSegment = Path.Combine("samples", "es-ES");
            Assert.That(samplesPath, Does.EndWith(expectedPathSegment));
        }

        #region RefreshDefaultGroupStyleNames

        private static readonly Guid ActionsStyleId = new Guid("4d68be4a-a04d-4945-9dd5-cdf61079d790");
        private static readonly Guid ReviewStyleId = new Guid("bc688959-ce34-4bf5-90f8-6ddd23f80989");

        private static void RunWithUICulture(string cultureName, Action action)
        {
            var thread = System.Threading.Thread.CurrentThread;
            var originalUICulture = thread.CurrentUICulture;
            var originalCulture = thread.CurrentCulture;
            var defaultNames = GroupStyleItem.DefaultGroupStyleItems.Select(style => style.Name).ToList();
            try
            {
                thread.CurrentUICulture = new System.Globalization.CultureInfo(cultureName);
                thread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
                action();
            }
            finally
            {
                thread.CurrentUICulture = originalUICulture;
                thread.CurrentCulture = originalCulture;
                // The refresh also renames the shared static defaults, so put them back for other tests.
                for (int i = 0; i < defaultNames.Count; i++)
                {
                    GroupStyleItem.DefaultGroupStyleItems[i].Name = defaultNames[i];
                }
            }
        }

        private static PreferenceSettings CreateSettingsWithEnglishDefaultStyles()
        {
            var settings = new PreferenceSettings();
            settings.GroupStyleItemsList = GroupStyleItem.DefaultGroupStyleItems
                .Select(style => new GroupStyleItem
                {
                    Name = style.Name,
                    HexColorString = style.HexColorString,
                    FontSize = style.FontSize,
                    GroupStyleId = style.GroupStyleId,
                    IsDefault = true
                })
                .ToList();
            // Names as a file saved by an English session would hold them.
            settings.GroupStyleItemsList.First(style => style.GroupStyleId == ActionsStyleId).Name = "Actions";
            settings.GroupStyleItemsList.First(style => style.GroupStyleId == ReviewStyleId).Name = "Review";
            return settings;
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUICultureIsGermanThenRefreshRenamesDefaultGroupStyles()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();

                Assert.AreEqual("Aktionen", settings.GroupStyleItemsList.First(style => style.GroupStyleId == ActionsStyleId).Name);
                Assert.AreEqual("Überprüfung", settings.GroupStyleItemsList.First(style => style.GroupStyleId == ReviewStyleId).Name);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRefreshRunsThenSharedDefaultGroupStyleNamesFollowTheUICulture()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();

                Assert.AreEqual("Aktionen", GroupStyleItem.DefaultGroupStyleItems.First(style => style.GroupStyleId == ActionsStyleId).Name);
                Assert.AreEqual("Aktionen", GroupStyleItem.CloneDefaultGroupStyleItems().First(style => style.GroupStyleId == ActionsStyleId).Name);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRefreshRunsThenCustomGroupStylesKeepTheirNames()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();
            settings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "My style", HexColorString = "#FF0000", FontSize = 36, GroupStyleId = Guid.NewGuid(), IsDefault = false });
            // A custom style that reuses the id of a default one must not be renamed either.
            settings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "Custom actions", HexColorString = "#00FF00", FontSize = 36, GroupStyleId = ActionsStyleId, IsDefault = false });

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();

                Assert.IsTrue(settings.GroupStyleItemsList.Any(style => style.Name == "My style" && !style.IsDefault));
                Assert.IsTrue(settings.GroupStyleItemsList.Any(style => style.Name == "Custom actions" && !style.IsDefault));
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenDefaultStyleHasUnknownIdThenRefreshLeavesItUntouched()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();
            settings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "Imported", HexColorString = "#0000FF", FontSize = 36, GroupStyleId = Guid.NewGuid(), IsDefault = true });

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();

                Assert.IsTrue(settings.GroupStyleItemsList.Any(style => style.Name == "Imported"));
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAnotherStyleAlreadyUsesTheLocalizedNameThenRefreshKeepsTheDefaultName()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();
            settings.GroupStyleItemsList.Add(new GroupStyleItem { Name = "Aktionen", HexColorString = "#FF00FF", FontSize = 36, GroupStyleId = Guid.NewGuid(), IsDefault = false });

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();

                Assert.AreEqual("Actions", settings.GroupStyleItemsList.First(style => style.GroupStyleId == ActionsStyleId).Name);
                Assert.AreEqual(settings.GroupStyleItemsList.Count, settings.GroupStyleItemsList.Select(style => style.Name).Distinct().Count());
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRefreshRunsTwiceThenTheSecondRunChangesNothing()
        {
            var settings = CreateSettingsWithEnglishDefaultStyles();

            RunWithUICulture("de-DE", () =>
            {
                settings.RefreshDefaultGroupStyleNames();
                var afterFirst = settings.GroupStyleItemsList.Select(style => style.Name).ToList();

                settings.RefreshDefaultGroupStyleNames();

                CollectionAssert.AreEqual(afterFirst, settings.GroupStyleItemsList.Select(style => style.Name).ToList());
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenMigratedLocaleDiffersFromSessionCultureThenNamesFollowTheSessionAndNextStartCatchesUp()
        {
            // Settings migrated from an older version: Locale is German, default names are still English.
            // The session culture was applied from the pre-migration settings, so it is English here.
            var migrated = CreateSettingsWithEnglishDefaultStyles();
            migrated.Locale = "de-DE";

            RunWithUICulture("en-US", () =>
            {
                migrated.RefreshDefaultGroupStyleNames();

                // The names match the language the session is running in, which is what gets saved on exit.
                Assert.AreEqual("Actions", migrated.GroupStyleItemsList.First(style => style.GroupStyleId == ActionsStyleId).Name);
            });

            // The next start applies the saved Locale before the refresh, so the names switch then.
            RunWithUICulture(migrated.Locale, () =>
            {
                migrated.RefreshDefaultGroupStyleNames();

                Assert.AreEqual("Aktionen", migrated.GroupStyleItemsList.First(style => style.GroupStyleId == ActionsStyleId).Name);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenGroupStyleIdIsNotADefaultThenLocalizedDefaultNameIsNull()
        {
            Assert.IsNull(GroupStyleItem.GetLocalizedDefaultName(Guid.NewGuid()));
        }

        #endregion

        [Test]
        [Category("UnitTests")]
        [TestCase("")]
        [TestCase("<PreferenceSettings><NotClosed>")]
        public void WhenSettingsFileCannotBeReadThenSaveInternalBacksItUpAndReplacesIt(string content)
        {
            WithUnreadableSettingsFile(content, (settings, path, backupRoot) =>
            {
                var saved = settings.SaveInternal(path);

                Assert.IsTrue(saved);
                var backupPath = Path.Combine(backupRoot, "profiles", Path.GetFileName(path) + ".bak");
                Assert.IsTrue(File.Exists(backupPath), "The unreadable file should have been copied.");
                Assert.AreEqual(content, File.ReadAllText(backupPath));
                Assert.IsTrue(PreferenceSettings.Load(path).IsCreatedFromValidFile, "The settings file should have been replaced.");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBackupAlreadyExistsThenSaveInternalUsesTheNextNumber()
        {
            WithUnreadableSettingsFile("<PreferenceSettings><NotClosed>", (settings, path, backupRoot) =>
            {
                var profileDirectory = Path.Combine(backupRoot, "profiles");
                var baseName = Path.Combine(profileDirectory, Path.GetFileName(path) + ".bak");
                Directory.CreateDirectory(profileDirectory);
                File.WriteAllText(baseName, "first");
                File.WriteAllText(baseName + "1", "second");

                var saved = settings.SaveInternal(path);

                Assert.IsTrue(saved);
                Assert.AreEqual("first", File.ReadAllText(baseName), "An existing backup should not be overwritten.");
                Assert.AreEqual("second", File.ReadAllText(baseName + "1"), "An existing backup should not be overwritten.");
                Assert.AreEqual("<PreferenceSettings><NotClosed>", File.ReadAllText(baseName + "2"));
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileWasReplacedThenLaterSavesDoNotBackItUpAgain()
        {
            WithUnreadableSettingsFile("<PreferenceSettings><NotClosed>", (settings, path, backupRoot) =>
            {
                Assert.IsTrue(settings.SaveInternal(path));
                Assert.IsTrue(settings.SaveInternal(path));

                var backups = Directory.GetFiles(Path.Combine(backupRoot, "profiles"));
                Assert.AreEqual(1, backups.Length, "Only the unreadable file should be backed up.");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUnreadableSettingsFileIsReplacedThenProfileIsNoLongerUnreadable()
        {
            WithUnreadableSettingsFile("<PreferenceSettings><NotClosed>", (settings, path, backupRoot) =>
            {
                Assert.IsTrue(settings.IsProfileUnreadable);

                Assert.IsTrue(settings.SaveInternal(path));

                Assert.IsFalse(settings.IsProfileUnreadable, "The profile on disk is valid after the save.");
                // IsCreatedFromValidFile keeps describing how the settings were loaded.
                Assert.IsFalse(settings.IsCreatedFromValidFile);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUnreadableSettingsFileIsNotReplacedThenProfileStaysUnreadable()
        {
            WithUnreadableSettingsFile("<PreferenceSettings><NotClosed>", (settings, path, backupRoot) =>
            {
                settings.BackupLocation = string.Empty;

                Assert.IsFalse(settings.SaveInternal(path));

                Assert.IsTrue(settings.IsProfileUnreadable);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBackupLocationIsNotSetThenSaveInternalDoesNotOverwriteTheUnreadableFile()
        {
            WithUnreadableSettingsFile("<PreferenceSettings><NotClosed>", (settings, path, backupRoot) =>
            {
                settings.BackupLocation = string.Empty;
                var logged = new List<ILogMessage>();
                settings.MessageLogged += logged.Add;

                var saved = settings.SaveInternal(path);

                Assert.IsFalse(saved);
                Assert.AreEqual("<PreferenceSettings><NotClosed>", File.ReadAllText(path));
                Assert.AreEqual(1, logged.Count, "The skipped save should have been logged.");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBackupCannotBeCreatedThenSaveInternalDoesNotOverwriteTheUnreadableFile()
        {
            const string content = "<PreferenceSettings><NotClosed>";
            WithUnreadableSettingsFile(content, (settings, path, backupRoot) =>
            {
                // A file where the backup folder should be makes creating the profiles folder fail.
                File.WriteAllText(backupRoot, string.Empty);
                var logged = new List<ILogMessage>();
                settings.MessageLogged += logged.Add;

                var saved = settings.SaveInternal(path);

                Assert.IsFalse(saved);
                Assert.AreEqual(content, File.ReadAllText(path));
                Assert.AreEqual(1, logged.Count, "The skipped save should have been logged.");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileIsReadableThenSaveInternalDoesNotBackItUp()
        {
            var originalTestPath = PreferenceSettings.DynamoTestPath;
            PreferenceSettings.DynamoTestPath = null;
            var backupRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            try
            {
                var path = GetNewFileNameOnTempPath(".xml");
                new PreferenceSettings().Save(path);
                var settings = PreferenceSettings.Load(path);
                Assert.IsTrue(settings.IsCreatedFromValidFile);
                settings.BackupLocation = backupRoot;

                var saved = settings.SaveInternal(path);

                Assert.IsTrue(saved);
                Assert.IsFalse(Directory.Exists(backupRoot), "A readable profile should not be backed up.");
            }
            finally
            {
                PreferenceSettings.DynamoTestPath = originalTestPath;
                if (Directory.Exists(backupRoot))
                {
                    Directory.Delete(backupRoot, true);
                }
            }
        }

        // Loads an unreadable settings file and points the backup location at a temporary folder.
        private void WithUnreadableSettingsFile(string content, Action<PreferenceSettings, string, string> test)
        {
            var originalTestPath = PreferenceSettings.DynamoTestPath;
            PreferenceSettings.DynamoTestPath = null;
            var backupRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            try
            {
                var path = GetNewFileNameOnTempPath(".xml");
                File.WriteAllText(path, content);
                var settings = PreferenceSettings.Load(path);
                Assert.IsFalse(settings.IsCreatedFromValidFile, "The file should have been rejected on load.");
                settings.BackupLocation = backupRoot;

                test(settings, path, backupRoot);
            }
            finally
            {
                PreferenceSettings.DynamoTestPath = originalTestPath;
                if (Directory.Exists(backupRoot))
                {
                    Directory.Delete(backupRoot, true);
                }
                if (File.Exists(backupRoot))
                {
                    File.Delete(backupRoot);
                }
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileDoesNotExistThenSaveInternalCreatesIt()
        {
            var originalTestPath = PreferenceSettings.DynamoTestPath;
            PreferenceSettings.DynamoTestPath = null;
            try
            {
                var path = GetNewFileNameOnTempPath(".xml");
                var settings = PreferenceSettings.Load(path);
                Assert.IsTrue(settings.IsCreatedFromValidFile);

                var saved = settings.SaveInternal(path);

                Assert.IsTrue(saved);
                Assert.IsTrue(File.Exists(path));
            }
            finally
            {
                PreferenceSettings.DynamoTestPath = originalTestPath;
            }
        }
    }
}
