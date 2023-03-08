using System.Collections.Generic;
using System.IO;
using Dynamo.Configuration;
using Dynamo.Models;
using NUnit.Framework;
using System.Linq;
using System;
using Dynamo.Interfaces;
using System.Reflection;
using Dynamo.Utilities;

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
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            var settings = PreferenceSettings.Load(settingsFilePath);

            Assert.IsNotNull(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetPythonTemplateFilePath ()
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
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, false);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.BackupInterval, PreferenceSettings.DefaultBackupInterval);
            Assert.AreEqual(settings.UseHardwareAcceleration, true);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);
            Assert.AreEqual(settings.DynamoPlayerFolderGroups.Count, 0);

            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same when saved with defaults
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), true);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, true);
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, false);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.BackupInterval, PreferenceSettings.DefaultBackupInterval);
            Assert.AreEqual(settings.UseHardwareAcceleration, true);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);
            Assert.AreEqual(settings.DynamoPlayerFolderGroups.Count, 0);

            // Change setting values
            settings.SetIsBackgroundPreviewActive("MyBackgroundPreview", false);
            settings.ShowCodeBlockLineNumber = false;
            settings.IsIronPythonDialogDisabled = true;
            settings.ShowTabsAndSpacesInScriptEditor = true;
            settings.DefaultPythonEngine = "CP3";
            settings.MaxNumRecentFiles = 24;
            settings.BackupInterval = 120000; //change to 2 minutes(120000 ms)
            settings.UseHardwareAcceleration = false;
            settings.EnableNodeAutoComplete = false;
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


            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same as last changed
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), false);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, false);
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, true);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, true);
            Assert.AreEqual(settings.DefaultPythonEngine, "CP3");
            Assert.AreEqual(settings.MaxNumRecentFiles, 24);
            Assert.AreEqual(settings.BackupInterval, 120000);
            Assert.AreEqual(settings.UseHardwareAcceleration, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, false);
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

            Assert.AreEqual(DynamoModel.BuiltInPackagesToken,token);
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
                    string a = "";
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
            string firstPropertyWithSameValue = checkDifference.Properties.Except(checkDifference.DifferentPropertyValues).ToList().FirstOrDefault();
            string defSettNumberFormat = defaultSettings.NumberFormat;
            string newSettNumberFormat = newSettings.NumberFormat;
            string failMessage = $"The file {newSettingslFilePath} exist: {newSettingsExist.ToString()} | DiffProps: {diffProps.ToString()} | TotProps: {totProps.ToString()} | Default Sett NumberFormat: {defSettNumberFormat} | New Sett NumberFormat: {newSettNumberFormat} | First Property with the same value {firstPropertyWithSameValue}";

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
    }
}
