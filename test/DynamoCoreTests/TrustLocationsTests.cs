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
            settings.SetTrustedLocations(new List<string>() {
                Path.Combine(TestDirectory, "ShouldNotExist"),
                ExecutingDirectory
            });

            TrustedLocationsManager.Instance.Initialize(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestTrustLocationManagerAPIs()
        {
            Assert.AreEqual(TrustedLocationsManager.Instance.TrustedLocations.Count, 2);

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocationsManager.Instance.IsTrustedLocation(".//Test");
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocationsManager.Instance.IsTrustedLocation(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocationsManager.Instance.IsTrustedLocation("");
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                TrustedLocationsManager.Instance.IsTrustedLocation(Path.Combine(TestDirectory,":"));
            });

            var doesNotExist = TrustedLocationsManager.Instance.TrustedLocations[0];
            Assert.Throws<FileNotFoundException>(() =>
            {
                TrustedLocationsManager.Instance.IsTrustedLocation(doesNotExist);
            });

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(TrustedLocationsManager.Instance.IsTrustedLocation(notTrusted));

            var trusted = TrustedLocationsManager.Instance.TrustedLocations[1];
            Assert.IsTrue(TrustedLocationsManager.Instance.IsTrustedLocation(trusted));

            Assert.IsFalse(TrustedLocationsManager.Instance.AddTrustedLocation(ExecutingDirectory));
            Assert.IsTrue(TrustedLocationsManager.Instance.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs")));

            Assert.AreEqual(3, TrustedLocationsManager.Instance.TrustedLocations.Count);
            Assert.IsTrue(TrustedLocationsManager.Instance.RemoveTrustedLocation(ExecutingDirectory));
            Assert.AreEqual(2, TrustedLocationsManager.Instance.TrustedLocations.Count);

            Assert.IsTrue(TrustedLocationsManager.Instance.RemoveTrustedLocation(Path.Combine(TestDirectory, "pkgs")));
            Assert.AreEqual(1, TrustedLocationsManager.Instance.TrustedLocations.Count);

            Assert.AreEqual(1, settings.TrustedLocations.Count);

            // Test that TrustedLocations (in preferenceSettings) are immutable.
            settings.TrustedLocations.Clear();
            Assert.AreEqual(1, settings.TrustedLocations.Count);

            Assert.IsFalse(TrustedLocationsManager.Instance.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd", "pkg.json")));
            Assert.IsFalse(TrustedLocationsManager.Instance.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));

            Assert.IsTrue(TrustedLocationsManager.Instance.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));
            Assert.IsTrue(TrustedLocationsManager.Instance.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));

            TrustedLocationsManager.Instance.SetTrustedLocations(new List<string>() { TestDirectory });

            Assert.IsTrue(TrustedLocationsManager.Instance.IsTrustedLocation(TestDirectory));
            Assert.AreEqual(1, TrustedLocationsManager.Instance.TrustedLocations.Count);
        }
    }
}
