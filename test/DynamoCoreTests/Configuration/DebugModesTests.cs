using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Dynamo.Configuration;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class DebugModesTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestLoadDebugModes()
        {
            string configPath = Path.Combine(TestDirectory, "testDebugModes", "debugModes.config");

            XmlDocument xml = new XmlDocument();
            xml.Load(configPath);
            var debugItems = xml.DocumentElement.SelectNodes("DebugMode");

            var testDebugModes = new Dictionary<string, bool>();
            foreach (XmlNode item in debugItems)
            {
                testDebugModes[item.Attributes["name"].Value] = bool.Parse(item.Attributes["enabled"].Value);
            }

            Type dbgModesType = typeof(DebugModes);
   
            // Register the test debug modes.
            MethodInfo addDebugMode = dbgModesType.GetMethod("AddDebugMode", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var dbgModeName in testDebugModes)
            {
                addDebugMode.Invoke(null, new object[] { dbgModeName.Key, dbgModeName.Key });
            }

            // Load the enabled/disabled status from the test config file.
            dbgModesType.GetMethod("LoadDebugModesStatusFromConfig", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { configPath });

            foreach (var item in testDebugModes)
            {
                var dbgMode = DebugModes.GetDebugMode(item.Key);
                Assert.IsNotNull(dbgMode);
                Assert.AreEqual(dbgMode.IsEnabled, item.Value);
            }

            var forceEnabled = false;
            foreach (var item in testDebugModes)
            {
                DebugModes.SetDebugModeEnabled(item.Key, forceEnabled);
            }

            foreach (var item in testDebugModes)
            {
                Assert.AreEqual(DebugModes.IsEnabled(item.Key), forceEnabled);
            }
        }
        [Test]
        [Category("UnitTests")]
        public void TestLoadNoDebugModes()
        {
            string configPath = Path.Combine(TestDirectory, "testDebugModes", "noDebugModes.config");

            XmlDocument xml = new XmlDocument();
            xml.Load(configPath);
            var debugItems = xml.DocumentElement.SelectNodes("DebugMode");

            Assert.IsEmpty(debugItems);

            Type dbgModesType = typeof(DebugModes);

            var testDebugModeNames = new List<string>() { "test5", "test6" };
            // Register the test debug modes.
            MethodInfo addDebugMode = dbgModesType.GetMethod("AddDebugMode", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var dbgModeName in testDebugModeNames)
            {
                addDebugMode.Invoke(null, new object[] { dbgModeName, dbgModeName });
            }

            // Load the enabled/disabled status from the test config file.
            dbgModesType.GetMethod("LoadDebugModesStatusFromConfig", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { configPath });

            foreach (var dbgModeName in testDebugModeNames)
            {
                Assert.IsNotNull(DebugModes.GetDebugMode(dbgModeName));
                Assert.AreEqual(DebugModes.IsEnabled(dbgModeName), false);
            }        
        }
        [Test]
        [Category("UnitTests")]
        public void TestMissingConfig()
        {
            Type dbgModesType = typeof(DebugModes);

            var testDebugModeNames = new List<string>() { "test7", "test8" };
            // Register the test debug modes.
            MethodInfo addDebugMode = dbgModesType.GetMethod("AddDebugMode", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var dbgModeName in testDebugModeNames)
            {
                addDebugMode.Invoke(null, new object[] { dbgModeName, dbgModeName });
            }

            // Load the enabled/disabled status from the test config file.
            string configPath = Path.Combine(TestDirectory, "testDebugModes", "missing.config");
            dbgModesType.GetMethod("LoadDebugModesStatusFromConfig", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { configPath });

            foreach (var dbgModeName in testDebugModeNames)
            {
                Assert.IsNotNull(DebugModes.GetDebugMode(dbgModeName));
                Assert.AreEqual(DebugModes.IsEnabled(dbgModeName), false);
            }
        }
    }
}
