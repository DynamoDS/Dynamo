using Dynamo.Configuration;
using Dynamo.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            Assert.AreEqual(settings.TrustedLocations.Count, 2,"trust location count is incorrect");

            Assert.IsFalse(settings.IsTrustedLocation(".//Test"),"relative path should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(null),"null should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(""),"empty should not be trusted");

            Assert.IsFalse(settings.IsTrustedLocation(Path.GetTempPath()), "temp should not be trusted by default");

            Assert.IsTrue(settings.AddTrustedLocation(Path.GetTempPath()), "temp should be added to trusted paths successfully");

            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, ":")),"random test directory subdirectory should not be trusted");

            var doesNotExist = settings.TrustedLocations[0];
            Assert.IsFalse(settings.IsTrustedLocation(doesNotExist),"trusted location must exist");

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(settings.IsTrustedLocation(notTrusted),"Parent of trusted should not be trusted.");

            var trusted = settings.TrustedLocations[1];
            Assert.IsTrue(settings.IsTrustedLocation(trusted),"executing dir should be trusted");

            Assert.IsFalse(settings.AddTrustedLocation(ExecutingDirectory),"should not be able to add path twice");
            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs")),"add test package dir to trust successfully");

            Assert.IsFalse(settings.AddTrustedLocation(Path.GetTempPath()),"cannot add tempt path twice to trusted paths");

            Assert.AreEqual(4, settings.TrustedLocations.Count,"should be 4 trusted paths");
            Assert.IsTrue(settings.RemoveTrustedLocation(ExecutingDirectory),"can remove executing dir from trust paths - loc 1");
            Assert.AreEqual(3, settings.TrustedLocations.Count,"3 paths remain");

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.GetTempPath()),$"can remove temp path");

            Assert.IsTrue(settings.RemoveTrustedLocation(Path.Combine(TestDirectory, "pkgs")),"can remove test pkg dir");
            Assert.AreEqual(1, settings.TrustedLocations.Count,"1 path remains");

            // Test that TrustedLocations (in preferenceSettings) are immutable.
            settings.TrustedLocations.Clear();
            Assert.AreEqual(1, settings.TrustedLocations.Count,"clearing external list copy does not modify property");

            Assert.IsFalse(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd", "pkg.json")),"cannot add file as path");
            Assert.IsFalse(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")),"parent of file is not trusted");

            Assert.IsTrue(settings.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")),"can add package folder as trusted path");
            Assert.IsTrue(settings.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")),"package folder is trusted after adding.");

            settings.SetTrustedLocations(new List<string>() { TestDirectory });

            Assert.IsTrue(settings.IsTrustedLocation(TestDirectory),"test dir is trusted after setting paths");
            Assert.AreEqual(1, settings.TrustedLocations.Count,"only one path is trusted");
        }
    }
}
