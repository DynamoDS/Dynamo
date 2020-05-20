using Dynamo.Configuration;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class AnalyticsServiceLimitModulesTest : DynamoModelTestBase
    {
        //We need to override this function because the one in DynamoModelTestBase is setting StartInTestMode = true
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = new PreferenceSettings() { IsAnalyticsReportingApproved = false, IsADPAnalyticsReportingApproved = false },
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        /// <summary>
        /// This test method will validate that the Google Analytics dll will not be loaded when the user opts out of all analtics.
        /// </summary>
        [Test]
        public void TestLoadedAssembliesOnStartup()
        {
            if (!DebugModes.IsEnabled("ADPAnalyticsTracker"))
                return;

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(o => o.GetName().Name).OrderBy(o => o).ToList();
            Assert.IsTrue(loadedAssemblies.Contains("Analytics.NET.Core"));
            Assert.IsTrue(loadedAssemblies.Contains("Analytics.Net.ADP"));
            Assert.IsFalse(loadedAssemblies.Contains("Analytics.NET.Google"));
        }     
    }
}
