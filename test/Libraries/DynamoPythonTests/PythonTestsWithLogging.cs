using System;
using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Models;
using NUnit.Framework;

namespace DynamoPythonTests
{
    /// <summary>
    /// Python tests that require real logging to be enabled (StartInTestMode = false)
    /// </summary>
    public class PythonTestsWithLogging : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCPython.dll");
            libraries.Add("DSIronPython.dll");
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = base.CreateStartConfiguration(settings);
            config.StartInTestMode = false;
            return config;
        }

        [Test]
        public void DynamoPrintLogsToConsole()
        {
            var expectedOutput = "Greeting CPython node: Hello from Python3!!!" + Environment.NewLine
                + "Greeting IronPython node: Hello from Python2!!!" + Environment.NewLine
                + "Greeting CPython String node: Hello from Python3!!!" + Environment.NewLine
                + "Greeting IronPython String node: Hello from Python2!!!" + Environment.NewLine
                + "Multiple print parameter node: Hello Dynamo Print !!!" + Environment.NewLine
                + "Print separator parameter node: Hello_Dynamo_Print_!!!" + Environment.NewLine
                + @"`!""£$%^&*()_+-[{]}#~'@;:|\,<.>/? Special character node: Lot's of special characters!!!" + Environment.NewLine;

            CurrentDynamoModel.OpenFileFromPath(Path.Combine(TestDirectory, "core", "python", "DynamoPrint.dyn"));
            StringAssert.EndsWith(expectedOutput, CurrentDynamoModel.Logger.LogText);
        }
    }
}
