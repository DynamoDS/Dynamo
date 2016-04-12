using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Tests;
using Dynamo.Updates;
using Moq;
using NUnit.Framework;

namespace Dynamo
{
    public class UserDataMigrationTests : UnitTestBase
    {
        private static void VerifySortedOrder(List<FileVersion> list)
        {
            for (int i = 0; i < list.Count() - 1; ++i)
            {
                var f1 = list[i];
                var f2 = list[i + 1];
                Assert.IsTrue(f1 >= f2);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void FileVersionMajorMinorParts()
        {
            var userDataRoot = "test";
            var fileVersion = new FileVersion(12, 34, userDataRoot);
            Assert.AreEqual(12, fileVersion.MajorPart);
            Assert.AreEqual(34, fileVersion.MinorPart);
            Assert.AreEqual(userDataRoot, fileVersion.UserDataRoot);
        }

        [Test]
        [Category("UnitTests")]
        public void FileVersionCompare_Sorting()
        {
            var fv1 = new FileVersion(0, 7, "x");
            var fv2 = new FileVersion(0, 8, "y");

            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(0, 0, "x");
            fv2 = new FileVersion(0, 0, "y");
            Assert.IsFalse(fv1 < fv2);
            Assert.IsFalse(fv1 > fv2);
            Assert.IsFalse(fv2 < fv1);
            Assert.IsFalse(fv2 > fv1);

            fv1 = new FileVersion(1, 7, "x");
            fv2 = new FileVersion(1, 8, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(1, 9, "x");
            fv2 = new FileVersion(2, 0, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-1, 9, "x");
            fv2 = new FileVersion(0, 0, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, 9, "x");
            fv2 = new FileVersion(-1, 0, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(2, -9, "x");
            fv2 = new FileVersion(3, 0, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(2, -9, "x");
            fv2 = new FileVersion(2, -8, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, -9, "x");
            fv2 = new FileVersion(-2, -8, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);

            fv1 = new FileVersion(-2, -9, "x");
            fv2 = new FileVersion(-1, -9, "y");
            Assert.IsTrue(fv1 < fv2);
            Assert.IsTrue(fv2 > fv1);
        }

        [Test]
        [Category("UnitTests")]
        public void SortFileVersionList_SortedList()
        {
            var list = new List<FileVersion>
            {
                new FileVersion(0, 7, "a"),
                new FileVersion(0, 8, "b"),
                new FileVersion(0, 0, "c"),
                new FileVersion(0, 0, "d"),
                new FileVersion(1, 7, "e"),
                new FileVersion(1, 8, "f"),
                new FileVersion(1, 9, "g"),
                new FileVersion(2, 0, "h"),
                new FileVersion(-1, 9, "x"),
                new FileVersion(0, 0, "y"),
                new FileVersion(-2, 9, "z"),
                new FileVersion(-1, 0, "a"),
                new FileVersion(2, -9, "b"),
                new FileVersion(3, 0, "c"),
                new FileVersion(2, -9, "d"),
                new FileVersion(2, -8, "e"),
                new FileVersion(-2, -9, "f"),
                new FileVersion(-2, -8, "g"),
                new FileVersion(-2, -9, "h"),
                new FileVersion(-1, -9, "asdad"),

            };

            list.Sort();

            VerifySortedOrder(list);
        }

        [Test, Category("UnitTests")]
        public void TestInstalledVersionFromUserDataFolder()
        {
            //Test with valid version string
            var tempPath = base.TempFolder;
            var userDataFolder = Path.Combine(tempPath, "1.0");
            var version = DynamoMigratorBase.GetInstallVersionFromUserDataFolder(userDataFolder);
            Assert.AreEqual(tempPath, version.UserDataRoot);
            Assert.AreEqual(1, version.MajorPart);
            Assert.AreEqual(0, version.MinorPart);

            //Test with -ve version string
            userDataFolder = Path.Combine(tempPath, "-1.-9");
            version = DynamoMigratorBase.GetInstallVersionFromUserDataFolder(userDataFolder);
            Assert.AreEqual(tempPath, version.UserDataRoot);
            Assert.AreEqual(-1, version.MajorPart);
            Assert.AreEqual(-9, version.MinorPart);

            //Test with invalid data
            version = DynamoMigratorBase.GetInstallVersionFromUserDataFolder(tempPath);
            Assert.AreEqual(default(FileVersion), version);
        }

        [Test]
        [Category("UnitTests")]
        public void GetSortedInstalledVersions_FromDirectoryInspection()
        {
            var tempPath = base.TempFolder;
            var uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.7"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "0.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.7"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "1.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "3.0"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.-9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "2.-8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.-9"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-2.-8"));
            uniqueDirectory = Directory.CreateDirectory(Path.Combine(tempPath, "-1.-9"));

            var versionList = DynamoMigratorBase.GetInstalledVersions(tempPath).ToList();

            VerifySortedOrder(versionList);

            try
            {
                DynamoMigratorBase.GetInstalledVersions("");
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.ParamName == "rootFolder");
            }
            
            try
            {
                DynamoMigratorBase.GetInstalledVersions(null);
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.ParamName == "rootFolder");                
            }
            
        }

        [Test, Category("UnitTests")]
        public void GetSortedInstallVersion_FromDynamoLookUp()
        {
            var mockLookup = new Mock<IDynamoLookUp>();
            mockLookup.Setup(x=>x.GetDynamoUserDataLocations()).Returns(MockUserDirectories);

            var versionList = DynamoMigratorBase.GetInstalledVersions(null, mockLookup.Object).ToList();
            VerifySortedOrder(versionList);
            Assert.AreEqual(4, versionList.Count);
            Assert.AreEqual(Path.Combine(TempFolder, "Test"), versionList[0].UserDataRoot);
            Assert.AreEqual(TempFolder, versionList[1].UserDataRoot);
        }

        [Test, Category("UnitTests")]
        public void GetLatestVersionToMigrate()
        {
            var mockLookup = new Mock<IDynamoLookUp>();
            mockLookup.Setup(x => x.GetDynamoUserDataLocations()).Returns(MockUserDirectories);

            var current = new FileVersion(1, 0, Path.Combine(TempFolder, "Test"));
            var latest = DynamoMigratorBase.GetLatestVersionToMigrate(null, mockLookup.Object, current);
            Assert.IsTrue(latest.HasValue);
            Assert.AreEqual(1, latest.Value.MajorPart);
            Assert.AreEqual(0, latest.Value.MinorPart);
            Assert.AreEqual(TempFolder, latest.Value.UserDataRoot);
        }

        [Test, Category("UnitTests")]
        public void GetLatestVersionToMigrate_10()
        {
            var mockLookup = new Mock<IDynamoLookUp>();
            mockLookup.Setup(x => x.GetDynamoUserDataLocations()).Returns(MockUserDirectories);

            var current = new FileVersion(1, 0, TempFolder);
            var latest = DynamoMigratorBase.GetLatestVersionToMigrate(null, mockLookup.Object, current);
            Assert.IsTrue(latest.HasValue);
            Assert.AreEqual(1, latest.Value.MajorPart);
            Assert.AreEqual(0, latest.Value.MinorPart);
            Assert.AreEqual(Path.Combine(TempFolder, "Test"), latest.Value.UserDataRoot);
        }


        [Test, Category("UnitTests")]
        public void GetLatestVersionToMigrate_PartiallyValidData()
        {
            var mockLookup = new Mock<IDynamoLookUp>();
            mockLookup.Setup(x => x.GetDynamoUserDataLocations()).Returns(MockUserDirectories);

            var current = new FileVersion(0, 9, TempFolder);
            var latest = DynamoMigratorBase.GetLatestVersionToMigrate(null, mockLookup.Object, current);
            Assert.IsTrue(latest.HasValue);
            Assert.AreEqual(0, latest.Value.MajorPart);
            Assert.AreEqual(8, latest.Value.MinorPart);
            Assert.AreEqual(TempFolder, latest.Value.UserDataRoot);
        }

        [Test, Category("UnitTests")]
        public void GetLatestVersionToMigrate_AllInvalidData()
        {
            var mockLookup = new Mock<IDynamoLookUp>();
            mockLookup.Setup(x => x.GetDynamoUserDataLocations()).Returns(new[] { "x", "y", "z" });

            var current = new FileVersion(1, 0, "a");
            var latest = DynamoMigratorBase.GetLatestVersionToMigrate(null, mockLookup.Object, current);
            Assert.IsTrue(!latest.HasValue || latest.Value.UserDataRoot == null);
        }

        private IEnumerable<string> MockUserDirectories()
        {
            yield return Path.Combine(TempFolder, "Test", "1.0");
            yield return Path.Combine(TempFolder, "0.8");
            yield return Path.Combine(TempFolder, "1.0");
            yield return Path.Combine(TempFolder, "0.9");
        }

        private static void CreateMockPreferenceSettingsFile(string filePath, string packageDir)
        {
            var settings = new PreferenceSettings
            {
                CustomPackageFolders = new List<string>{packageDir}
            };
            settings.Save(filePath);
        }

        /// <summary>
        /// Create 0.8, and 0.9 version directories in Temp dir
        /// Create dummy package and definitions files in 0.8 dir
        /// Create empty packages and definitions dir in 0.9 folder
        /// </summary>
        /// <param name="userDataDir">return root folder</param>
        private void CreateMockDirectoriesAndFiles(out string userDataDir)
        {
            userDataDir = Path.Combine(TempFolder, "DynamoMigration");

            var tempFolder = Path.Combine(userDataDir, "0.8");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            tempFolder = Path.Combine(userDataDir, "0.8", "packages");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            using(File.Create(Path.Combine(tempFolder, "package1.dll"))) { }

            tempFolder = Path.Combine(userDataDir, "0.8", "definitions");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            using(File.Create(Path.Combine(tempFolder, "definition1.dyn"))) { }

            tempFolder = Path.Combine(userDataDir, "0.9");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            tempFolder = Path.Combine(userDataDir, "0.9", "packages");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            tempFolder = Path.Combine(userDataDir, "0.9", "definitions");

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

        }

        [Test]
        [Category("UnitTests")]
        public void TestMigration()
        {
            // Create 0.8, and 0.9 version user data directories in Temp folder
            string userDataDir;
            CreateMockDirectoriesAndFiles(out userDataDir);

            var sourceVersionDir = Path.Combine(userDataDir, "0.8");
            var settingsFilePath = Path.Combine(sourceVersionDir, "DynamoSettings.xml");

            // Create PreferenceSettings.xml file in 0.8 
            CreateMockPreferenceSettingsFile(settingsFilePath, sourceVersionDir);

            // Create mock objects for IPathManager and IPathResolver
            var mockPathManager = new Mock<IPathManager>();
            
            var currentVersionDir = Path.Combine(userDataDir, "0.9");

            mockPathManager.Setup(x => x.UserDataDirectory).Returns(() => currentVersionDir);

            // Test MigrateBetweenDynamoVersions
            var targetMigrator = DynamoMigratorBase.MigrateBetweenDynamoVersions(
                mockPathManager.Object);

            // Assert that both 0.8 and 0.9 dirs are the same after migration
            var sourcePackageDir = Path.Combine(sourceVersionDir, "packages");
            var currentPackageDir = Path.Combine(currentVersionDir, "packages");

            bool areDirectoriesEqual = Directory.EnumerateFiles(sourcePackageDir).Select(Path.GetFileName).
                SequenceEqual(Directory.EnumerateFiles(currentPackageDir).Select(Path.GetFileName));
            Assert.IsTrue(areDirectoriesEqual);

            var sourceDefinitionDir = Path.Combine(sourceVersionDir, "definitions");
            var currentDefinitionDir = Path.Combine(currentVersionDir, "definitions");

            areDirectoriesEqual = Directory.EnumerateFiles(sourceDefinitionDir).Select(Path.GetFileName).
                SequenceEqual(Directory.EnumerateFiles(currentDefinitionDir).Select(Path.GetFileName));
            Assert.IsTrue(areDirectoriesEqual);

            // Assert that new CustomePackageFolders in preference settings
            // for 0.9 version points to user data dir for 0.9
            Assert.AreEqual(currentVersionDir,
                targetMigrator.PreferenceSettings.CustomPackageFolders[0]);
        }
    }
}
