using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.NUnit.Tests;
using NUnit.Core;
using NUnit.Core.Filters;

namespace Dynamo.Tests
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class DynamoTestFramework : IExternalCommand
    {
        private UIDocument m_doc;
        private UIApplication m_revit;
        private resultType testResult;

        /// <summary>
        /// Test name specified in journal's data map.
        /// If test name is specified, one test will be run by name.
        /// </summary>
        private string testName = "";

        /// <summary>
        /// Fixture name specified in journal's data map.
        /// If fixture name is specified, all tests in fixture will be run.
        /// </summary>
        private string fixtureName = "";

        /// <summary>
        /// Test assembly location specified in journal's data map.
        /// The test assembly location must be specified in order to find tests to run.
        /// </summary>
        private string testAssembly = "";

        /// <summary>
        /// The directory where the results file will be written. 
        /// </summary>
        private string resultsPath = "";

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            //Get the data map from the running journal file.
            IDictionary<string, string> dataMap = revit.JournalData;

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                bool canReadData = (0 < dataMap.Count);

                if (canReadData)
                {
                    if (dataMap.ContainsKey("testName"))
                    {
                        testName = dataMap["testName"];
                    }
                    if (dataMap.ContainsKey("fixtureName"))
                    {
                        fixtureName = dataMap["fixtureName"];
                    }
                    if (dataMap.ContainsKey("testAssembly"))
                    {
                        testAssembly = dataMap["testAssembly"];
                    }
                    if (dataMap.ContainsKey("resultsPath"))
                    {
                        resultsPath = dataMap["resultsPath"];
                    }
                }

                if (string.IsNullOrEmpty(testAssembly))
                {
                    throw new Exception("Test assembly location must be specified in journal.");
                }

                if (string.IsNullOrEmpty(resultsPath))
                {
                    throw new Exception("You must supply a path for the results file.");
                }

                //http://stackoverflow.com/questions/2798561/how-to-run-nunit-from-my-code

                //Tests must be executed on the main thread in order to access the Revit API.
                //NUnit's SimpleTestRunner runs the tests on the main thread
                //http://stackoverflow.com/questions/16216011/nunit-c-run-specific-tests-through-coding?rq=1
                CoreExtensions.Host.InitializeService();
                var runner = new SimpleTestRunner();
                var builder = new TestSuiteBuilder();
                var package = new TestPackage("DynamoTestFramework", new List<string>() { testAssembly });
                runner.Load(package);
                TestSuite suite = builder.Build(package);
                TestFixture fixture = null;
                FindFixtureByName(out fixture, suite, fixtureName);
                if (fixture == null)
                    throw new Exception(string.Format("Could not find fixture: {0}", fixtureName));

                //foreach (var t in fixture.Tests)
                //{
                //    if (t is ParameterizedMethodSuite)
                //    {
                //        var paramSuite = t as ParameterizedMethodSuite;
                //        foreach (var tInner in paramSuite.Tests)
                //        {
                //            if (tInner is TestMethod)
                //                Results.Results.Add(new DynamoRevitTest(tInner as TestMethod));
                //        }
                //    }
                //    else if (t is TestMethod)
                //        Results.Results.Add(new DynamoRevitTest(t as TestMethod));
                //}

                InitializeResults();

                if (!canReadData)
                {
                    var currInvalid = Convert.ToInt16(testResult.invalid);
                    testResult.invalid = currInvalid + 1;
                    testResult.testsuite.result = "Error";

                    throw new Exception("Journal file's data map contains no information about tests.");
                }

                var cases = testResult.testsuite.results.Items.ToList();

                //for testing
                //if the journal file contains data

                TestMethod t = FindTestByName(fixture, dataMap["testName"]);
                if (t != null)
                {
                    TestFilter filter = new NameFilter(t.TestName);
                    var result = (t as TestMethod).Run(new TestListener(), filter);

                    //result types
                    //Ignored, Failure, NotRunnable, Error, Success

                    var testCase = new testcaseType
                        {
                            name = t.TestName.Name,
                            executed = result.Executed.ToString(),
                            success = result.IsSuccess.ToString(),
                            asserts = result.AssertCount.ToString(CultureInfo.InvariantCulture),
                            time = result.Time.ToString(CultureInfo.InvariantCulture)
                        };
                    testResult.testsuite.success = true.ToString();

                    var currAsserts = Convert.ToInt16(testResult.testsuite.asserts);
                    testResult.testsuite.asserts = (currAsserts + result.AssertCount).ToString();

                    var currCount = Convert.ToInt16(testResult.total);
                    testResult.total = (currCount + 1);

                    if (result.IsSuccess)
                    {
                        testCase.result = "Success";
                    }
                    else if (result.IsFailure)
                    {
                        var fail = new failureType {message = result.Message, stacktrace = result.StackTrace};
                        testCase.Item = fail;
                        testCase.result = "Failure";
                        testResult.testsuite.success = false.ToString();
                        testResult.testsuite.result = "Failure";
                    }
                    else if (result.IsError)
                    {
                        var errCount = Convert.ToInt16(testResult.errors);
                        testResult.errors = (errCount + 1);
                        testCase.result = "Error";
                        testResult.testsuite.result = "Failure";
                    }

                    cases.Add(testCase);
                }
                else
                {
                    //we have a journal file, but the specified test could not be found
                    var currInvalid = Convert.ToInt16(testResult.invalid);
                    testResult.invalid = currInvalid + 1;
                    testResult.testsuite.result = "Error";
                }

                testResult.testsuite.results.Items = cases.ToArray();

                SaveResults();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Sets up an NUNit ResultsType object or deserializing and existing one.
        /// </summary>
        private void InitializeResults()
        {
            if (File.Exists(resultsPath))
            {
                //read from the file
                var x = new XmlSerializer(typeof(resultType));
                using (var sr = new StreamReader(resultsPath))
                {
                    testResult = (resultType)x.Deserialize(sr);
                }
            }
            else
            {
                //create one result to dump everything into
                testResult = new resultType {name = Assembly.GetExecutingAssembly().Location};

                var suite = new testsuiteType
                    {
                        name = "DynamoTestFrameworkTests",
                        description = "Unit tests in Revit.",
                        time = "0.0",
                        type = "TestFixture",
                        result = "Success",
                        executed = "True"
                    };

                testResult.testsuite = suite;
                testResult.testsuite.results = new resultsType {Items = new object[] {}};

                testResult.date = DateTime.Now.ToString("yyyy-MM-dd");
                testResult.time = DateTime.Now.ToString("HH:mm:ss");
                testResult.failures = 0;
                testResult.ignored = 0;
                testResult.notrun = 0;
                testResult.errors = 0;
                testResult.skipped = 0;
                testResult.inconclusive = 0;
                testResult.invalid = 0;
            }
        }

        /// <summary>
        /// Serializes the results to an NUnit compatible xml file.
        /// </summary>
        private void SaveResults()
        {
            //write to the file
            var x = new XmlSerializer(typeof(resultType));
            using (var tw = XmlWriter.Create(resultsPath, new XmlWriterSettings() { Indent = true }))
            {
                tw.WriteComment("This file represents the results of running a test suite");
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                x.Serialize(tw, testResult, ns);
            }
        }

        /// <summary>
        /// Find an NUnit test fixture by name.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="suite"></param>
        /// <param name="name"></param>
        private static void FindFixtureByName(out TestFixture fixture, TestSuite suite, string name)
        {
            foreach (TestSuite innerSuite in suite.Tests)
            {
                if (innerSuite is TestFixture)
                {
                    if (((TestFixture)innerSuite).TestName.Name == name)
                    {
                        fixture = innerSuite as TestFixture;
                        return;
                    }
                }
                else
                {
                    FindFixtureByName(out fixture, innerSuite, name);
                    if (fixture != null)
                        return;
                }
            }

            fixture = null;
        }

        /// <summary>
        /// Find an NUnit test method within a given fixture by name.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TestMethod FindTestByName(TestFixture fixture, string name)
        {
            return (from t in fixture.Tests.OfType<TestMethod>() 
                    where (t as TestMethod).TestName.Name == name 
                    select t as TestMethod).FirstOrDefault();
        }
    }
}
