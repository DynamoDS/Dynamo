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
            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedLocation(doesNotExist);
            });

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(TrustedLocatationsManager.Instance.IsTrustedLocation(notTrusted));

            var trusted = TrustedLocatationsManager.Instance.TrustedLocations[1];
            Assert.IsTrue(TrustedLocatationsManager.Instance.IsTrustedLocation(trusted));

        }
    }
}
