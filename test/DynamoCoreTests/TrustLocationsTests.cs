using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Configuration;
using DynamoUtilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class TrustLocationsTests : UnitTestBase
    {
        private PreferenceSettings settings;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            settings = new PreferenceSettings();
            settings.SetTrustedLocationsUnsafe(new List<string>() {
                Path.Combine(TestDirectory, "ShouldNotExist"),
                ExecutingDirectory
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestTrustLocationManagerAPIs()
        {
            Assert.AreEqual(settings.TrustedLocations.Count, 2, "trust location count is incorrect");

            Assert.IsFalse(settings.IsTrustedLocation(".//Test"), "relative path should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(null), "null should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(""), "empty should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(Path.GetTempPath()), "temp should not be trusted by default");

            Assert.IsTrue(settings.AddTrustedLocation(Path.GetTempPath()), "temp should be added to trusted paths successfully");

            Assert.IsTrue(settings.IsTrustedLocation(Path.GetTempPath()), "temp should now be trusted");

            var tempWithSuffix = Path.GetTempPath() + "2222";
            Assert.IsFalse(settings.TrustedLocations.Contains(tempWithSuffix), "temp with suffix should not be in list of paths");
            Assert.IsFalse(settings.IsTrustedLocation(tempWithSuffix), "temp with suffix should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, ":")), "sibling path should not be trusted");

            var doesNotExist = settings.TrustedLocations[0];
            Assert.IsFalse(settings.IsTrustedLocation(doesNotExist), "trusted location must exist");

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(settings.IsTrustedLocation(notTrusted), "Parent of trusted should not be trusted.");

            var trusted = settings.TrustedLocations[1];
            Assert.IsTrue(settings.IsTrustedLocation(trusted), "executing dir should be trusted");

            Assert.IsFalse(settings.AddTrustedLocation(ExecutingDirectory), "should not be able to add path twice");
            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs")), "add test package dir to trust successfully");

            Assert.IsFalse(settings.AddTrustedLocation(Path.GetTempPath()), "cannot add tempt path twice to trusted paths");

            Assert.AreEqual(4, settings.TrustedLocations.Count, "should be 4 trusted paths");
            Assert.IsTrue(settings.RemoveTrustedLocation(ExecutingDirectory), "can remove executing dir from trust paths - loc 1");
            Assert.AreEqual(3, settings.TrustedLocations.Count, "3 paths remain");

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.GetTempPath()), $"can remove temp path");

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.Combine(TestDirectory, "pkgs")), "can remove test pkg dir");
            Assert.AreEqual(1, settings.TrustedLocations.Count, "1 path remains");

            // Test that TrustedLocations (in preferenceSettings) are immutable.
            settings.TrustedLocations.Clear();
            Assert.AreEqual(1, settings.TrustedLocations.Count, "clearing external list copy does not modify property");

            Assert.IsFalse(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd", "pkg.json")), "cannot add file as path");
            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")), "parent of file is not trusted");

            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")), "can add package folder as trusted path");
            Assert.IsTrue(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")), "package folder is trusted after adding.");

            settings.SetTrustedLocations(new List<string>() { TestDirectory });

            Assert.IsTrue(settings.IsTrustedLocation(TestDirectory), "test dir should have been set as trusted");
            Assert.AreEqual(1, settings.TrustedLocations.Count, "set trusted should have set only 1 path");

            var rootDir = Path.GetPathRoot(System.Environment.SystemDirectory);
            Assert.IsTrue(settings.AddTrustedLocation(rootDir), $"adding {rootDir} (root dir of this machine) should succeed");
            Assert.IsFalse(settings.AddTrustedLocation($"{rootDir}users"), "adding a subdirectory of already trusted path should fail.");
            Assert.IsFalse(settings.AddTrustedLocation($"{rootDir}Users\\pinzart\\AppData\\Local\\Temp\\1"), "adding path that does not exist should fail. ");
            Assert.IsTrue(settings.IsTrustedLocation(System.Environment.SystemDirectory), $"root volume should be trusted, so all sub folders should be trusted.root was{rootDir}, sub dir was {System.Environment.SystemDirectory}");
        }

        [Test]
        [Category("UnitTests")]
        public void TestPathHelper()
        {
            string dir1 = "C:\\users";
            string dir2 = "C:\\users" + Path.DirectorySeparatorChar;
            string dir3 = "C:\\users" + Path.AltDirectorySeparatorChar;
            Assert.IsTrue(PathHelper.AreDirectoryPathsEqual(dir1, dir2));
            Assert.IsTrue(PathHelper.AreDirectoryPathsEqual(dir1, dir3));
            Assert.IsTrue(PathHelper.AreDirectoryPathsEqual(dir1.ToUpper(), dir3.ToLower()));

            string parentDir1 = @"C:\\B";
            string parentDir2 = @"C:\\B\\";
            string subDir1 = @"C:\B\C\D\C\E\F\G";
            string subDir2 = @"C:\D\C\D\C\E\F\G";
            Assert.IsTrue(PathHelper.IsSubDirectoryOfDirectory(subDir1, parentDir1));
            Assert.IsTrue(PathHelper.IsSubDirectoryOfDirectory(subDir1, parentDir2));

            Assert.IsFalse(PathHelper.IsSubDirectoryOfDirectory(subDir2, parentDir1));
            Assert.IsFalse(PathHelper.IsSubDirectoryOfDirectory(subDir2, parentDir2));
        }
    }
}
