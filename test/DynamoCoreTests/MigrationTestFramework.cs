﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    [Category("JsonTestExclude")]
    class MigrationTestFramework : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// Automated creation of regression test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        [Test, TestCaseSource("SetupMigrationTests"), Category("RegressionTests")]
        public void Regressions(string dynamoFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
            
            //open the dyn file
            OpenModel(dynamoFilePath);

            AssertNoDummyNodes();
            //run the expression and assert that it does not
            //throw an error
            Assert.DoesNotThrow(BeginRun);

        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        static List<string> SetupMigrationTests()
        {
            var testParameters = new List<string>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\core\migration\");
            var regTestPath = Path.GetFullPath(testsLoc);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                if (fileInfo.FullName.Contains("FAILURE"))
                    continue;

                testParameters.Add(fileInfo.FullName);        
            }

            return testParameters;
        }
        
    }
}
