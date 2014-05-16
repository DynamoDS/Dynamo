using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using NUnit.Framework;

using ProtoCore.Mirror;
using Dynamo.Nodes;
using DSCoreNodesUI;
using Dynamo.Utilities;


namespace Dynamo.Tests
{
    [TestFixture]
    class MigrationTestFramework : Dynamo.Tests.DSEvaluationUnitTest
    {
        /// <summary>
        /// Automated creation of regression test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        [Test, TestCaseSource("SetupMigrationTests")]
        public void Regressions(string dynamoFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
            
            //open the dyn file
            Assert.True(dynSettings.Controller.DynamoViewModel.OpenCommand.CanExecute(dynamoFilePath));
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(dynamoFilePath);


            AssertNoDummyNodes();
            //run the expression and assert that it does not
            //throw an error
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            GetPreviewValues();
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        static List<string> SetupMigrationTests()
        {
            //dynSettings.DynamoLogger.Log("Setting up migration tests...", LogLevel.File);

            var testParameters = new List<string>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\core\migration\");
            var regTestPath = Path.GetFullPath(testsLoc);

            //dynSettings.DynamoLogger.Log(string.Format("Using regression path: {0}", regTestPath), LogLevel.File);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                testParameters.Add(fileInfo.FullName);

                //dynSettings.DynamoLogger.Log(fileInfo.FullName.ToString(), LogLevel.File);                
            }

            return testParameters;
        }

        #region Private Helper Methods


        #endregion
    }
}
