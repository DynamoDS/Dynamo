using System;
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
        public void LoadMultiplePackageLocationsFromSetting()
        {
            var filePath = Path.Combine(SettingDirectory, "duplicatePaths_DynamoSettings.xml");
            var settings = PreferenceSettings.Load(filePath);

            Assert.NotNull(settings);
            Assert.AreEqual(4, settings.PackageFolders.Count);
            Assert.AreEqual(3, settings.CustomNodeFolders.Count);

            var expectedPackageFolders = new List<string> {@"C:\21", @"C:\46", @"C:\55", @"C:\17"};

            IEnumerable<bool> comparisonResult = settings.PackageFolders.Zip(expectedPackageFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));

            var expectedCustomNodeFolders = new List<string> {@"D:\17", @"D:\46", @"D:\55"};

            comparisonResult = settings.CustomNodeFolders.Zip(expectedCustomNodeFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));
        }

        [Test]
        public void LoadInvalidLocationsFromSetting()
        {
            var filePath = Path.Combine(SettingDirectory, "invalidPaths_DynamoSettings.xml");
            var settings = PreferenceSettings.Load(filePath);

            Assert.NotNull(settings);
            Assert.AreEqual(4, settings.PackageFolders.Count);
            Assert.AreEqual(3, settings.CustomNodeFolders.Count);

            var expectedPackageFolders = new List<string> { @"C:\folder_name_with_invalid_:*?|_characters\foobar",
                                                               @"C:\this_folder_doesn't_exist",
                                                               @"X:\this_drive_doesn't_exist",
                                                               @"\\unreachable_machine\share_packages" };

            IEnumerable<bool> comparisonResult = settings.PackageFolders.Zip(expectedPackageFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));

            var expectedCustomNodeFolders = new List<string> { @"\\test_machine\non-existent_folder",
                                                                  @"D:\custom_nodes",
                                                                  @"E:\this_folder_doesn't_exist" };

            comparisonResult = settings.CustomNodeFolders.Zip(expectedCustomNodeFolders, string.Equals);
            Assert.IsFalse(comparisonResult.Any(isEqual => !isEqual));
        }

    }

}
