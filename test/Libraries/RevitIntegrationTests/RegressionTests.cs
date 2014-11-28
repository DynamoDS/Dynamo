using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.DB;

using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Models;
using Dynamo.ViewModels;

using DynamoUtilities;

using NUnit.Framework;

using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;

namespace RevitSystemTests
{
    [TestFixture]
    public class RegressionTest : SystemTest
    {
        /// <summary>
        /// Automated creation of regression test cases. Opens each workflow
        /// runs it, and checks for errors or warnings. Regression test cases should
        /// be structured such that they do not yield warnings or errors.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        /// <param name="revitFilePath">The path of the Revit rfa or rvt file.</param>
        [Test]
        [TestCaseSource("SetupRevitRegressionTests")]
        public void Regressions(RegressionTestData testData)
        {
            Exception exception = null;

            try
            {
                var dynamoFilePath = testData.Arguments[0].ToString();
                var revitFilePath = testData.Arguments[1].ToString();

                //ensure that the incoming arguments are not empty or null
                //if a dyn file is found in the regression tests directory
                //and there is no corresponding rfa or rvt, then an empty string
                //or a null will be passed into here.
                Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
                Assert.IsNotNullOrEmpty(revitFilePath, "Revit file path is invalid or missing.");

                //open the revit model
                SwapCurrentModel(revitFilePath);

                //Setup should be called after swapping document, so that RevitDynamoModel 
                //is now associated with swapped model.
                Setup();

                //open the dyn file
                ViewModel.OpenCommand.Execute(dynamoFilePath);
                Assert.IsTrue(ViewModel.Model.Nodes.Count > 0);
                AssertNoDummyNodes();
                
                //run the expression and assert that it does not
                //throw an error
                Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
                var errorNodes =
                    ViewModel.Model.Nodes.Where(
                        x => x.State == ElementState.Error || x.State == ElementState.Warning);
                Assert.AreEqual(0, errorNodes.Count());
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                ViewModel.Model.ShutDown(false);
                ViewModel = null;
                TearDown();
            }

            if (exception != null)
            {
                Assert.Fail(exception.Message);
            }
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        private static List<RegressionTestData> SetupRevitRegressionTests()
        {
            var testParameters = new List<RegressionTestData>();

			var config = RevitTestConfiguration.LoadConfiguration();
            string testsLoc = Path.Combine(config.WorkingDirectory, "Regression");
            var regTestPath = Path.GetFullPath(testsLoc);

            var di = new DirectoryInfo(regTestPath);
            foreach (var folder in di.GetDirectories())
            {
                var dyns = folder.GetFiles("*.dyn");
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

                    testParameters.Add(new RegressionTestData{Arguments=data, TestName = folder.Name});
                }
            }
            

            return testParameters;
        }
    }

    public class RegressionTestData
    {
        public object[] Arguments { get; set; }
        public string TestName { get; set; }
        public override string ToString()
        {
            return TestName ?? "RegressionTest";
        }
    }
}
