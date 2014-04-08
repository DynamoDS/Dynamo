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
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class RevitTestFramework : IExternalCommand
    {
        #region private members

        private resultType resultsRoot;

        public testsuiteType rootSuite
        {
            get { return resultsRoot.testsuite; }
        }

        public resultsType dynamoResults
        {
            get { return resultsRoot.testsuite.results; }
        }

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

        /// <summary>
        /// Should we attach to the debugger?
        /// </summary>
        private bool isDebug;

        #endregion

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Dynamo.Utilities.AssemblyHelper.CurrentDomain_AssemblyResolve;

            try
            {
                var docManager = DocumentManager.Instance;
                docManager.CurrentUIApplication = revit.Application;

                //Get the data map from the running journal file.
                IDictionary<string, string> dataMap = revit.JournalData;

                bool canReadData = (0 < dataMap.Count);

                ReadDataFromJournalHash(canReadData, dataMap);

                if (string.IsNullOrEmpty(testAssembly))
                {
                    throw new Exception("Test assembly location must be specified in journal.");
                }

                if (string.IsNullOrEmpty(resultsPath))
                {
                    throw new Exception("You must supply a path for the results file.");
                }

                if (isDebug)
                {
                    Debugger.Launch();
                }

                var fixtureResult = RunTests(canReadData);

                CalculateCaseTotalsOnSuite(fixtureResult);
                CalculateSweetTotalsOnOuterSweet(rootSuite);
                CalculateTotalsOnResultsRoot(resultsRoot);

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

        private testsuiteType RunTests(bool canReadData)
        {
            //http://stackoverflow.com/questions/2798561/how-to-run-nunit-from-my-code

            //Tests must be executed on the main thread in order to access the Revit API.
            //NUnit's SimpleTestRunner runs the tests on the main thread
            //http://stackoverflow.com/questions/16216011/nunit-c-run-specific-tests-through-coding?rq=1
            CoreExtensions.Host.InitializeService();
            var runner = new SimpleTestRunner();
            var builder = new TestSuiteBuilder();
            string testAssemblyLoc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), testAssembly);

            var package = new TestPackage("RevitTestFramework", new List<string>() {testAssemblyLoc});
            runner.Load(package);
            TestSuite suite = builder.Build(package);

            TestFixture fixture = null;
            FindFixtureByName(out fixture, suite, fixtureName);
            if (fixture == null)
                throw new Exception(string.Format("Could not find fixture: {0}", fixtureName));

            InitializeResults();

            // If we can't read data, add a failed test result to root.
            if (!canReadData)
            {
                var currInvalid = Convert.ToInt16(resultsRoot.invalid);
                resultsRoot.invalid = currInvalid + 1;
                resultsRoot.testsuite.result = "Error";

                throw new Exception("Journal file's data map contains no information about tests.");
            }

            //find or create a fixture
            var fixtureResult = FindOrCreateFixtureResults(dynamoResults, fixtureName);

            //convert the fixture's results array to a list
            var runningResults = fixtureResult.results.Items.ToList();

            //if the test name is not specified
            //run all tests in the fixture
            if (string.IsNullOrEmpty(testName) || testName == "None")
            {
                var fixtureResults = RunFixture(fixture);
                runningResults.AddRange(fixtureResults);
            }
            else
            {
                var t = FindTestByName(fixture, testName);
                if (t != null)
                {
                    if (t is ParameterizedMethodSuite)
                    {
                        var paramSuite = t as ParameterizedMethodSuite;
                        runningResults.AddRange(
                            paramSuite.Tests.OfType<TestMethod>()
                                .Select(RunTest).Cast<object>());
                    }
                    else
                    {
                        var method = t as TestMethod;
                        if (method != null)
                        {
                            runningResults.Add(RunTest(method));
                        }
                    }
                }
                else
                {
                    //we have a journal file, but the specified test could not be found
                    var currInvalid = Convert.ToInt16(resultsRoot.invalid);
                    resultsRoot.invalid = currInvalid + 1;
                    resultsRoot.testsuite.result = "Error";
                }
            }

            fixtureResult.results.Items = runningResults.ToArray();
            return fixtureResult;
        }

        private void ReadDataFromJournalHash(bool canReadData, IDictionary<string, string> dataMap)
        {
            if (!canReadData) return;

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
            if (dataMap.ContainsKey("debug"))
            {
                try
                {
                    isDebug = Convert.ToBoolean(dataMap["debug"]);
                }
                catch
                {
                    isDebug = false;
                }
            }
        }

        //private void StartDynamo()
        //{
        //    var fecLevel = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
        //    fecLevel.OfClass(typeof(Level));

        //    DocumentManager.Instance.CurrentUIApplication = DocumentManager.Instance.CurrentUIApplication;
        //    //DocumentManager.Instance.CurrentUIDocument = DocumentManager.Instance.CurrentUIDocument;
        //    dynRevitSettings.DefaultLevel = null;

        //    BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
        //    BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
        //    BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

        //    //create dynamo
        //    var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
        //    string context = r.Replace(DocumentManager.Instance.CurrentUIApplication.Application.VersionName, "");

        //    // create the transaction manager object
        //    TransactionManager.SetupManager(new AutomaticTransactionStrategy());

        //    var dynamoController = new DynamoController_Revit(DynamoRevit.Updater, typeof(DynamoRevitViewModel), context);
        //    DynamoController.IsTestMode = true;
        //}

        private void CalculateTotalsOnResultsRoot(resultType result)
        {
            var resultItems = result.testsuite.results.Items.ToList();
            var cases = resultItems
                .Where(x => x.GetType() == typeof (testsuiteType))
                .Cast<testsuiteType>()
                .SelectMany(x => x.results.Items)
                .Where(x => x.GetType() == typeof (testcaseType))
                .Cast<testcaseType>();

            var testcaseTypes = cases as testcaseType[] ?? cases.ToArray();

            if (!testcaseTypes.Any())
                return;

            result.errors = testcaseTypes.Sum(x => x.result == "Failure" ? 1 : 0);
            result.total = testcaseTypes.Count();

            result.ignored = testcaseTypes.Sum(x => x.result == "Ignored" ? 1 : 0);
            result.inconclusive = testcaseTypes.Sum(x => x.result == "Inconclusive" ? 1 : 0);
            result.invalid = testcaseTypes.Sum(x => x.result == "Invalid" ? 1 : 0);
            result.notrun = testcaseTypes.Sum(x => x.executed == "NotRun" ? 1 : 0);
            result.skipped = testcaseTypes.Sum(x => x.result == "Skipped" ? 1 : 0);
            result.errors = testcaseTypes.Sum(x => x.result == "Error" ? 1 : 0);
            result.failures = testcaseTypes.Sum(x => x.result == "Failure" ? 1 : 0);
        }

        private void CalculateSweetTotalsOnOuterSweet(testsuiteType suite)
        {
            var suiteItems = suite.results.Items.ToList();
            var innerSuites = suiteItems.Where(x => x.GetType() == typeof (testsuiteType)).Cast<testsuiteType>();
            suite.asserts = innerSuites.Sum(x => Convert.ToInt16(x.asserts)).ToString();
            suite.result = innerSuites.Any(x => x.result == "Failure") ? "Failure" : "Success";
            suite.time = innerSuites.Sum(x => Convert.ToDouble(x.time)).ToString();
        }

        private void CalculateCaseTotalsOnSuite(testsuiteType suite)
        {
            //result types
            //Ignored, Failure, NotRunnable, Error, Success

            //success is true or false

            var suiteItems = suite.results.Items.ToList();
            var cases = suiteItems.Where(x => x.GetType() == typeof(testcaseType)).Cast<testcaseType>();
            suite.asserts = cases.Sum(x => Convert.ToInt16(x.asserts)).ToString();
            suite.result = cases.Any(x => x.result == "Failure") ? "Failure" : "Success";
            suite.time = cases.Sum(x => Convert.ToDouble(x.time)).ToString();
            //suite.executed = cases.All(x => x.executed == "False") ? "False" : "True";
            suite.executed = true.ToString();
        }

        /// <summary>
        /// Find or create a fixture in a suite by name.
        /// </summary>
        /// <param name="suite"></param>
        /// <returns></returns>
        private testsuiteType FindOrCreateFixtureResults(resultsType results, string fixtureName)
        {
            testsuiteType fixture = null;

            if (results.Items != null && results.Items.Any())
            {
                fixture = (testsuiteType)results.Items.
                FirstOrDefault(x => x.GetType() == typeof(testsuiteType) && ((testsuiteType)x).name == fixtureName);

                if (fixture != null)
                {
                    return fixture;
                }
            }

            fixture = new testsuiteType
                {
                    name = fixtureName,
                    description = "Unit tests in Revit.",
                    time = "0.0",
                    type = "TestFixture",
                    result = "Success",
                    executed = "True",
                    results = new resultsType { Items = new object[] { } }
                };

            //add the newly created fixture to the list of fixtures
            List<object> currentFixtures = null;
            currentFixtures = results.Items != null ? 
                results.Items.ToList() : 
                new List<object>();
            currentFixtures.Add(fixture);
            results.Items = currentFixtures.ToArray();

            return fixture;
        }

        /// <summary>
        /// Run all tests in a fixture.
        /// </summary>
        /// <param name="fixture">Returns a list of results. These results are usually testcaseType objects.</param>
        /// <returns></returns>
        private IEnumerable<object> RunFixture(TestFixture fixture)
        {
            var results = new List<object>();
            
            foreach (var t in fixture.Tests)
            {
                if (t is ParameterizedMethodSuite)
                {
                    var paramSuite = t as ParameterizedMethodSuite;
                    foreach (var tInner in paramSuite.Tests)
                    {
                        if (tInner is TestMethod)
                        {
                            results.Add(RunTest((TestMethod)tInner));
                        }
                    }
                }
                else if (t is TestMethod)
                {
                    results.Add(RunTest((TestMethod)t));
                }
            }

            return results;
        }

        private testcaseType RunTest(TestMethod t)
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

            switch (result.ResultState)
            {
                case ResultState.Cancelled:
                    testCase.result = "Cancelled";
                    break;
                case ResultState.Error:
                    var f = new failureType {message = result.Message, stacktrace = result.StackTrace};
                    testCase.Item = f;
                    testCase.result = "Error";
                    break;
                case ResultState.Failure:
                    var fail = new failureType {message = result.Message, stacktrace = result.StackTrace};
                    testCase.Item = fail;
                    testCase.result = "Failure";
                    break;
                case ResultState.Ignored:
                    testCase.result = "Ignored";
                    break;
                case ResultState.Inconclusive:
                    testCase.result = "Inconclusive";
                    break;
                case ResultState.NotRunnable:
                    testCase.result = "NotRunnable";
                    break;
                case ResultState.Skipped:
                    testCase.result = "Skipped";
                    break;
                case ResultState.Success:
                    testCase.result = "Success";
                    break;
            }

            return testCase;
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
                    resultsRoot = (resultType)x.Deserialize(sr);
                }
            }
            else
            {
                //create one result to dump everything into
                resultsRoot = new resultType { name = Assembly.GetExecutingAssembly().Location };

                resultsRoot.testsuite = new testsuiteType
                    {
                        name = "DynamoTestFrameworkTests",
                        description = "Unit tests in Revit.",
                        time = "0.0",
                        type = "TestFixture",
                        result = "Success",
                        executed = "True"
                    };

                resultsRoot.testsuite = rootSuite;
                resultsRoot.testsuite.results = new resultsType { Items = new object[] { } };
                resultsRoot.date = DateTime.Now.ToString("yyyy-MM-dd");
                resultsRoot.time = DateTime.Now.ToString("HH:mm:ss");
                resultsRoot.failures = 0;
                resultsRoot.ignored = 0;
                resultsRoot.notrun = 0;
                resultsRoot.errors = 0;
                resultsRoot.skipped = 0;
                resultsRoot.inconclusive = 0;
                resultsRoot.invalid = 0;
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
                x.Serialize(tw, resultsRoot, ns);
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
        private static Test FindTestByName(TestFixture fixture, string name)
        {
            return (from t in fixture.Tests.OfType<Test>()
                    where t.TestName.Name == name 
                    select t).FirstOrDefault();
        }
    }
}
