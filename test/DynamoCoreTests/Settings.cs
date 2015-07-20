﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class PreferenceSettingTests : DynamoModelTestBase
    {
        public string SettingDirectory { get { return Path.Combine(TestDirectory, "settings"); } }
        
        [Test]
        public void LoadInvalidLocationsFromSetting()
        {
            var filePath = Path.Combine(SettingDirectory, "invalidPaths_DynamoSettings.xml");
            var settings = PreferenceSettings.Load(filePath);

            Assert.NotNull(settings);
            Assert.AreEqual(4, settings.CustomPackageFolders.Count);

            var expectedPackageFolders = new List<string> { @"C:\folder_name_with_invalid_:*?|_characters\foobar",
                                                            @"C:\this_folder_doesn't_exist",
                                                            @"X:\this_drive_doesn't_exist",
                                                            @"\\unreachable_machine\share_packages" };

            IEnumerable<bool> comparisonResult = settings.CustomPackageFolders.Zip(expectedPackageFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));
        }

    }

}
