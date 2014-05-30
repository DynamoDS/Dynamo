﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Applications;
using Dynamo.Models;
using Dynamo.Utilities;
using DynamoUnits;
using NUnit.Framework;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;

namespace Dynamo.Tests
{
    [TestFixture]
    public class RegressionTest
    {
        protected static Transaction _trans;
        protected static string _testPath;
        protected static string _samplesPath;
        protected static string _defsPath;
        protected static string _emptyModelPath1;
        protected static string _emptyModelPath;
        public static DynamoController Controller;

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

                Setup();

                //ensure that the incoming arguments are not empty or null
                //if a dyn file is found in the regression tests directory
                //and there is no corresponding rfa or rvt, then an empty string
                //or a null will be passed into here.
                Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
                Assert.IsNotNullOrEmpty(revitFilePath, "Revit file path is invalid or missing.");

                //open the revit model
                SwapCurrentModel(revitFilePath);

                var model = Controller.DynamoModel;

                //open the dyn file
                model.Open(dynamoFilePath);

                //run the expression and assert that it does not
                //throw an error
                Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
                var errorNodes =
                    dynSettings.Controller.DynamoModel.Nodes.Where(
                        x => x.State == ElementState.Error || x.State == ElementState.Warning);
                Assert.AreEqual(0, errorNodes.Count());
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                Controller.ShutDown(false);
                Controller = null;
                Teardown();
            }

            if (exception != null)
            {
                Assert.Fail(exception.Message);
            }
        }

        private void Setup()
        {
            StartDynamo();

            DocumentManager.Instance.CurrentUIApplication.ViewActivating += CurrentUIApplication_ViewActivating;

            //it doesn't make sense to do these steps before every test
            //but when running from the revit plugin we are not loading the 
            //fixture, so the initfixture method is not called.

            //get the test path
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\System\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(assDir, @".\dynamo_packages\Dynamo Sample Custom Nodes\dyf\");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");

            if (DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber.Contains("2014") &&
                DocumentManager.Instance.CurrentUIApplication.Application.VersionName.Contains("Vasari"))
            {
                _emptyModelPath = Path.Combine(_testPath, "emptyV.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "emptyV1.rfa");
            }
            else
            {
                _emptyModelPath = Path.Combine(_testPath, "empty.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "empty1.rfa");
            }
        }

        private void Teardown()
        {
            // Automatic transaction strategy requires that we 
            // close the transaction if it hasn't been closed by 
            // by the end of an evaluation. It is possible to 
            // run the test framework without running Dynamo, so
            // we ensure that the transaction is closed here.
            TransactionManager.Instance.ForceCloseTransaction();
        }

        private void StartDynamo()
        {
            try
            {
                var updater = new RevitServicesUpdater(DynamoRevitApp.ControlledApplication, DynamoRevitApp.Updaters);
                updater.ElementAddedForID += ElementMappingCache.GetInstance().WatcherMethodForAdd;
                updater.ElementsDeleted += ElementMappingCache.GetInstance().WatcherMethodForDelete;

                SIUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
                SIUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
                SIUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

                var logger = new DynamoLogger();
                dynSettings.DynamoLogger = logger;
                var updateManager = new UpdateManager.UpdateManager(logger);

                Controller = DynamoRevit.CreateDynamoRevitControllerAndViewModel(updater, logger, Context.NONE);
                DynamoController.IsTestMode = true;

                // create the transaction manager object
                TransactionManager.SetupManager(new AutomaticTransactionStrategy());

                // Because the test framework does not work in the idle thread. 
                // We need to trick Dynamo into believing that it's in the idle
                // thread already.
                IdlePromise.InIdleThread = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        private List<RegressionTestData> SetupRevitRegressionTests()
        {
            var testParameters = new List<RegressionTestData>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\System\revit\Regression\");
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

        private void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument.Document;
            DocumentManager.Instance.CurrentUIApplication.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
        }

        void CurrentUIApplication_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            DynamoRevit.SetRunEnabledBasedOnContext(e);
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
