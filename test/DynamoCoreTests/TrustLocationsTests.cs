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
