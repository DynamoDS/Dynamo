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
            settings.TrustedLocations = new List<string>() {
                Path.Combine(TestDirectory, "ShouldNotExist"),
                ExecutingDirectory
            };

            TrustedLocatationsManager.Instance.Initialize(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestManagerAPIs()
        {
            Assert.AreEqual(TrustedLocatationsManager.Instance.TrustedLocations.Count, 2);

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation(".//Test");
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation("");
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation(Path.Combine(TestDirectory,":"));
            });

            var doesNotExist = TrustedLocatationsManager.Instance.TrustedLocations[0];
            Assert.Throws<FileNotFoundException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation(doesNotExist);
            });

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(TrustedLocatationsManager.Instance.IsTrustedLocation(notTrusted));

            var trusted = TrustedLocatationsManager.Instance.TrustedLocations[1];
            Assert.IsTrue(TrustedLocatationsManager.Instance.IsTrustedLocation(trusted));

            Assert.IsFalse(TrustedLocatationsManager.Instance.AddTrustedLocation(ExecutingDirectory));
            Assert.IsTrue(TrustedLocatationsManager.Instance.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs")));

            Assert.AreEqual(3, TrustedLocatationsManager.Instance.TrustedLocations.Count);
            Assert.IsTrue(TrustedLocatationsManager.Instance.RemoveTrustedLocation(ExecutingDirectory));
            Assert.AreEqual(2, TrustedLocatationsManager.Instance.TrustedLocations.Count);

            Assert.IsTrue(TrustedLocatationsManager.Instance.RemoveTrustedLocation(Path.Combine(TestDirectory, "pkgs")));
            Assert.AreEqual(1, TrustedLocatationsManager.Instance.TrustedLocations.Count);

            int settingsCount = 0;
            foreach(var item in settings.TrustedLocations)
                settingsCount++;

            Assert.AreEqual(1, settingsCount);

            Assert.IsTrue(TrustedLocatationsManager.Instance.AddTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd", "pkg.json")));
            Assert.IsTrue(TrustedLocatationsManager.Instance.IsTrustedLocation(Path.Combine(TestDirectory, "pkgs", "EvenOdd")));
        }
    }
}
