using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageLoaderCustomTest : DynamoModelTestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var settings = new PreferenceSettings();
            var parentFolder = Path.Combine(TestDirectory, "pkgs", "multiple_locations");
            settings.CustomPackageFolders = new List<string>
            {
                Path.Combine(parentFolder, "folder1"),
                @"C:\folder_name_with_invalid_:*?|_characters\foobar",
                @"C:\this_folder_doesn't_exist",
                Path.Combine(parentFolder, "folder2"),
                @"X:\this_drive_doesn't_exist",
                @"\\unreachable_machine\share_packages",
                Path.Combine(parentFolder, "folder3")
            };

            var settingFilePath = Path.Combine(TempFolder, "DynamoSettings.xml");
            settings.Save(settingFilePath);

            var settingsLoadedFromFile = PreferenceSettings.Load(settingFilePath);

            StartDynamo(settingsLoadedFromFile);
        }

        [Test]
        public void LoadFromMultipleLocationTest()
        {
            var loader = GetPackageLoader();
            
            Assert.AreEqual(loader.DefaultPackagesDirectory,
                Path.Combine(TestDirectory, "pkgs", "multiple_locations", "folder1", "packages"));

            Assert.AreEqual(2, loader.LocalPackages.Count());

            var pkg = loader.LocalPackages.ElementAt(0);

            Assert.AreEqual("CAAD_RWTH", pkg.Group);
            Assert.AreEqual("Custom Rounding", pkg.Name);
            Assert.AreEqual("0.1.4", pkg.VersionName);
            Assert.AreEqual("This collection of nodes allows rounding, rounding up and rounding down to a specified precision.", pkg.Description);
            Assert.AreEqual("Round Up To Precision - Rounds a number *up* to a specified precision, Round Down To Precision - "
                + "Rounds a number *down* to a specified precision, Round To Precision - Rounds a number to a specified precision", pkg.Contents);
            Assert.AreEqual("0.5.2.10107", pkg.EngineVersion);

            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);

            var nextPkg = loader.LocalPackages.ElementAt(1);

            Assert.AreEqual("CAAD_RWTH", nextPkg.Group);
            Assert.AreEqual("GetHighest", nextPkg.Name);
            Assert.AreEqual("0.1.2", nextPkg.VersionName);
            Assert.AreEqual("Gets the highest value from a list", nextPkg.Description);
            Assert.AreEqual("Get Highest - Gets the highest value from a list", nextPkg.Contents);
            Assert.AreEqual("0.5.2.10107", nextPkg.EngineVersion);

            Assert.AreEqual(1, nextPkg.LoadedCustomNodes.Count);
        }
    }
}
