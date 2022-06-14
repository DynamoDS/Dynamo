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
        [Category("UnitTests"), Category("Failure")]
        public void TestTrustLocationManagerAPIs()
        {
            Assert.AreEqual(settings.TrustedLocations.Count, 2);

            Assert.IsFalse(settings.IsTrustedLocation(".//Test"));

            Assert.IsFalse(settings.IsTrustedLocation(null));

            Assert.IsFalse(settings.IsTrustedLocation(""));

            Assert.IsFalse(settings.IsTrustedLocation(Path.GetTempPath()));

            Assert.IsTrue(settings.AddTrustedLocation(Path.GetTempPath()));

            Assert.IsTrue(settings.IsTrustedLocation(Path.GetTempPath()));

            var tempWithSuffix = Path.GetTempPath() + "2222";
            Assert.IsFalse(settings.TrustedLocations.Contains(tempWithSuffix));
            Assert.IsFalse(settings.IsTrustedLocation(tempWithSuffix));

            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, ":")));

            var doesNotExist = settings.TrustedLocations[0];
            Assert.IsFalse(settings.IsTrustedLocation(doesNotExist));

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(settings.IsTrustedLocation(notTrusted));

            var trusted = settings.TrustedLocations[1];
            Assert.IsTrue(settings.IsTrustedLocation(trusted));

            Assert.IsFalse(settings.AddTrustedLocation(ExecutingDirectory));
            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs")));

            Assert.IsFalse(settings.AddTrustedLocation(Path.GetTempPath()));

            Assert.AreEqual(4, settings.TrustedLocations.Count);
            Assert.IsTrue(settings.RemoveTrustedLocation(ExecutingDirectory));
            Assert.AreEqual(3, settings.TrustedLocations.Count);

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.GetTempPath()));

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.Combine(TestDirectory, "pkgs")));
            Assert.AreEqual(1, settings.TrustedLocations.Count);

            // Test that TrustedLocations (in preferenceSettings) are immutable.
            settings.TrustedLocations.Clear();
            Assert.AreEqual(1, settings.TrustedLocations.Count);

            Assert.IsFalse(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd", "pkg.json")));
            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));

            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));
            Assert.IsTrue(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));

            settings.SetTrustedLocations(new List<string>() { TestDirectory });

            Assert.IsTrue(settings.IsTrustedLocation(TestDirectory));
            Assert.AreEqual(1, settings.TrustedLocations.Count);

            Assert.IsTrue(settings.AddTrustedLocation(@"C:\"));
            Assert.IsFalse(settings.AddTrustedLocation(@"C:\users"));
            Assert.IsFalse(settings.AddTrustedLocation(@"C:\Users\pinzart\AppData\Local\Temp\1"));
            Assert.IsTrue(settings.IsTrustedLocation(Path.GetTempPath()));
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
