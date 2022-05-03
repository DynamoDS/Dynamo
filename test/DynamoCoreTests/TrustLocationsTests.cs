using Dynamo.Configuration;
using Dynamo.Core;
using NUnit.Framework;
using System;
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
            settings.TrustedLocations.Add(Path.Combine(TestDirectory, "ShouldNotExist"));
            settings.TrustedLocations.Add(ExecutingDirectory);

            TrustedLocatationsManager.Instance.Initialize(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestManagerAPIs()
        {
            Assert.AreEqual(TrustedLocatationsManager.Instance.TrustedLocations.Count, settings.TrustedLocations.Count);

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedFolder(".//Test");
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedFolder(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedFolder("");
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedFolder(Path.Combine(TestDirectory,":"));
            });

            var doesNotExist = TrustedLocatationsManager.Instance.TrustedLocations[0];
            Assert.Throws<ArgumentException>(() =>
            {
                TrustedLocatationsManager.Instance.IsTrustedFolder(doesNotExist);
            });

            var notTrusted = Directory.GetParent(doesNotExist).FullName;
            Assert.IsFalse(TrustedLocatationsManager.Instance.IsTrustedFolder(notTrusted));

            var trusted = TrustedLocatationsManager.Instance.TrustedLocations[1];
            Assert.IsTrue(TrustedLocatationsManager.Instance.IsTrustedFolder(trusted));

        }
    }
}
