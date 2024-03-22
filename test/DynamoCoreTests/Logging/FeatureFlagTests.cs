using Dynamo.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Dynamo.Tests.Logging
{
    [TestFixture]
     class FeatureFlagTests
    {
        [SetUp]
        public void Setup()
        {
            eventCounter = 0;
            log = string.Empty;
        }

        int eventCounter=0;
        string log = string.Empty;
        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldReturnRealDataAfterCache()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.CacheAllFlags();
            Assert.IsTrue(testflagsManager.CheckFeatureFlag<bool>("TestFlag1", false));
        }

        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldReturnDefaultDataBeforeCache()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            Assert.False(testflagsManager.CheckFeatureFlag<bool>("TestFlag1", false));

        }
        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldReturnValidStringData()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.CacheAllFlags();
            Assert.AreEqual("I am a string", testflagsManager.CheckFeatureFlag<string>("TestFlag2", "NA"));

        }

        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldTriggerEventAfterCacheFlags()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved += DynamoFeatureFlagsManager_FlagsRetrieved;
            testflagsManager.CacheAllFlags();

            DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved -= DynamoFeatureFlagsManager_FlagsRetrieved;
            Assert.AreEqual(1, eventCounter);
        }
        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldMessageLoggedShouldContainAllLogs()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.MessageLogged += TestflagsManager_MessageLogged;
            testflagsManager.CacheAllFlags();

            testflagsManager.MessageLogged -= TestflagsManager_MessageLogged;
            //json ordering cannot be controlled between .net target versions.
            StringAssert.StartsWith("LD startup: testmode true, no LD connection. LD startup time:" , log);
            StringAssert.Contains("<<<<<InitDone>>>>>feature flag exe starting<<<<<Sod>>>>>",log);
            StringAssert.Contains("\"TestFlag1\":true", log);
            StringAssert.Contains("\"TestFlag2\":\"I am a string\"", log);
            StringAssert.Contains("\"graphics-primitive-instancing\":true", log);
            StringAssert.EndsWith("<<<<<Eod>>>>>", log);

        }
        //TODO(DYN-6464) Revisit this and log more when the logger is not easily overwhelmed.
        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldMessageLoggedShouldOnlyContainNullFlagErrorOnce()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.MessageLogged += TestflagsManager_MessageLogged;
            testflagsManager.CheckFeatureFlag("TestFlag2", "na");
            testflagsManager.CheckFeatureFlag("TestFlag2", "na");
            testflagsManager.CheckFeatureFlag("TestFlag2", "na");
            testflagsManager.MessageLogged -= TestflagsManager_MessageLogged;
            var matches = Regex.Matches(log, "wait longer for the cache").Count;
            Assert.AreEqual(1,matches);
        }
        //TODO(DYN-6464) Revisit this and log more when the logger is not easily overwhelmed.
        [Test]
        [Category("Failure")]
        public void FeatureFlagsShouldMessageLoggedShouldOnlyContainMissingFlagErrorOnce()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.MessageLogged += TestflagsManager_MessageLogged;
            testflagsManager.CacheAllFlags();
            testflagsManager.CheckFeatureFlag("MissingFlag", "na");
            testflagsManager.CheckFeatureFlag("MissingFlag", "na");
            testflagsManager.CheckFeatureFlag("MissingFlag", "na");
            testflagsManager.MessageLogged -= TestflagsManager_MessageLogged;
            var matches = Regex.Matches(log, "failed to get value").Count;
            Assert.AreEqual(1, matches);
        }
        [Test]
        [Category("Failure")]
        public void FeatureFlagsThrowsIfCheckIngNonSupportedType()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            Assert.Throws<ArgumentException>(() =>
            {
                testflagsManager.CheckFeatureFlag("NumericTypeNotSupported", 10);
            });
        }

        private void DynamoFeatureFlagsManager_FlagsRetrieved()
        {
            eventCounter++;
        }
        private void TestflagsManager_MessageLogged(string message)
        {
            log = log + message;
        }

    }
}
