using Dynamo.Logging;
using Dynamo.Updates;
using Moq;
using NUnit.Framework;
using DynUpdateManager = Dynamo.Updates.UpdateManager;
using Dynamo.Configuration;
using Dynamo.Core;
using System.Collections.Generic;
using System;
using Dynamo.Interfaces;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class DynamoLoggerTest : DynamoModelTestBase
    {
        private const string DOWNLOAD_SOURCE_PATH_S = "http://downloadsourcepath/";
        private const string SIGNATURE_SOURCE_PATH_S = "http://SignatureSourcePath/";
        private PathManager pathManager;

        /// <summary>
        /// This test method will check the LogEventArgs.LogEventArgs
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_dynamologger_LogEventArgs()
        {
            //Arrange/Act
            var logEventArgs = new LogEventArgs(new Exception("Test Exception Message"),LogLevel.Console);

            //Arrange
            Assert.IsTrue(logEventArgs.Message.Contains("Test Exception Message"));
            Assert.AreEqual(logEventArgs.Level, LogLevel.Console);
        }


        /// <summary>
        /// This test method will check several methods inside the DynamoLogger class:
        /// LogWarning
        /// ResetWarning
        /// LogInfo
        /// LogError
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_dynamologger_logger()
        {
            //Arrange
            //This has been done in the base class but we need to do it again because the pathManager is private
            var config = CreateStartConfiguration(dynamoSettings);
            pathManager = new PathManager(new PathManagerParams
            {
                CorePath = config.DynamoCorePath,
                HostPath = config.DynamoHostPath,
                PathResolver = config.PathResolver
            });

            // Ensure we have all directories in place.
            var exceptions = new List<Exception>();
            pathManager.EnsureDirectoryExistence(exceptions);

            //By setting the VerboseLogging we will enable a piece of code inside the method::
            //private void Log(string message, LogLevel level, bool reportModification)
            var debugSettings = new DebugSettings
            {
                VerboseLogging = true
            };
            var logger = new DynamoLogger(debugSettings, pathManager.LogDirectory);

            //Act
            logger.LogWarning("Testing Logging Warning", WarningLevel.Error);
            Assert.AreEqual(logger.WarningLevel, WarningLevel.Error);
            Assert.AreEqual(logger.Warning, "Testing Logging Warning");

            logger.ResetWarning();
            logger.LogInfo("Testing Logging Warning", "info test");
            logger.LogError("Testing Logging Error","Error Details");

            //Assert
            //Check if the logged info was inserted inside the logged text (the log file cannot be opened due that is being used by the base class)
            Assert.IsTrue(logger.LogText.Contains("Testing Logging Warning"));

            logger.Dispose();
            logger.Dispose();//Calling a second time this function will enter in a validation not covered
        }
    }
}
