using Dynamo.Logging;
using NUnit.Framework;
using System;
using System.IO;

namespace Dynamo.Tests.Loggings
{
    /// <summary>
    /// This class was created to call the missing coverage protected methods in the base class
    /// </summary>
    class MyLogger : LogSourceBase
    {
        public void LogError(Exception ex)
        {
            Log(ex);//Execute LogSource.LogError(string error)
        }

        public void LogMessage(string msg, WarningLevel wl)
        {
            Log(msg, wl);//Execute LogSource.LogWarning(string warning, WarningLevel level)
        }
    }

    /// <summary>
    /// This test methods will execute the next LogSource methods:
    /// DispatchedLogger.Log(string tag, string message)
    /// DispatchedLogger.LogError(string error)
    /// </summary>
    [TestFixture]
    class LogSourceTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void Test_DispatchedLogger_Log()
        {
            //Arrange
            var logSource = new MyLogger();

            //Act
            var dispachedLogger = logSource.AsLogger();

            //Assert
            Assert.IsNotNull(dispachedLogger);

            dispachedLogger.LogError("Error Message");
            dispachedLogger.Log("TagTest", "Error Message");
        }
    }
}
