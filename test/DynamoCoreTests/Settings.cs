﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Configuration;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class PreferenceSettingTests : DynamoModelTestBase
    {
        public string SettingDirectory { get { return Path.Combine(TestDirectory, "settings"); } }
        
        [Test]
        public void LoadInvalidLocationsFromSetting()
        {
            var filePath = Path.Combine(SettingDirectory, "DynamoSettings-invalidPaths.xml");

            // check files required for test exist
            Assert.IsTrue(File.Exists(filePath));

            // load the settings from XML file into DynamoModel
            var settings = PreferenceSettings.Load(filePath);

            // check settings were read correctly
            Assert.NotNull(settings);
            Assert.AreEqual(4, settings.CustomPackageFolders.Count);

            var expectedPackageFolders = new List<string> { @"C:\folder_name_with_invalid_:*?|_characters\foobar",
                                                            @"C:\this_folder_doesn't_exist",
                                                            @"X:\this_drive_doesn't_exist",
                                                            @"\\unreachable_machine\share_packages" };

            IEnumerable<bool> comparisonResult = settings.CustomPackageFolders.Zip(expectedPackageFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));
        }

        [Test]
        public void LoadInvalidPythonTemplateFromSetting()
        {
            var settingsFilePath = Path.Combine(SettingDirectory, "DynamoSettings-invalidPaths.xml");
            var pyFile = @"C:\this_folder_doesn't_exist\PythonTemplate.py";

            // check files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            // load the settings from XML file into DynamoModel
            var settings = PreferenceSettings.Load(settingsFilePath);

            // check settings were read correctly
            Assert.NotNull(settings);
            Assert.IsFalse(File.Exists(settings.PythonTemplateFilePath));
            Assert.IsFalse(File.Exists(pyFile));
            Assert.AreEqual(settings.PythonTemplateFilePath, pyFile);
        }

    }

}
