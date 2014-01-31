using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class RegressionTests:DynamoRevitUnitTestBase
    {
        /// <summary>
        /// Automated creation of regression test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        /// <param name="revitFilePath">The path of the Revit rfa or rvt file.</param>
        [Test, TestCaseSource("SetupRevitRegressionTests")]
        public void Regressions(string dynamoFilePath, string revitFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            //and there is no corresponding rfa or rvt, then an empty string
            //or a null will be passed into here.
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
            Assert.IsNotNullOrEmpty(revitFilePath, "Revit file path is invalid or missing.");
            
            //open the revit model
            SwapCurrentModel(revitFilePath);

            //open the dyn file
            Assert.True(dynSettings.Controller.DynamoViewModel.OpenCommand.CanExecute(dynamoFilePath));
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(dynamoFilePath);

            //run the expression and assert that it does not
            //throw an error
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(false));
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        static List<object[]> SetupRevitRegressionTests()
        {
            DynamoLogger.Instance.Log("Setting up regression tests...", LogLevel.File);

            var testParameters = new List<object[]>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\revit\Regression\");
            var regTestPath = Path.GetFullPath(testsLoc);

            DynamoLogger.Instance.Log(string.Format("Using regression path: {0}", regTestPath), LogLevel.File);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                var data = new object[2];
                data[0] = fileInfo.FullName;

                //find the corresponding rfa or rvt file
                var nameBase = fileInfo.FullName.Remove(fileInfo.FullName.Length - 4);
                var rvt = nameBase + ".rvt";
                var rfa = nameBase + ".rfa";

                //add test parameters for rvt, rfa, or both
                if (File.Exists(rvt))
                {
                    data[1] = rvt;
                }

                if (File.Exists(rfa))
                {
                    data[1] = rfa;
                }

                testParameters.Add(data);

                DynamoLogger.Instance.Log(data[0].ToString(),LogLevel.File);
                DynamoLogger.Instance.Log(data[0].ToString(), LogLevel.File);
            }

            return testParameters;
        }
    }
}
