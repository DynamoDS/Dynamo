using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Tests;
using Dynamo.Utilities;
using NDesk.Options;
using NUnit.Framework;

namespace DynamoTestFrameworkRunner
{
    class Program
    {
        private static string _testAssembly = null;
        private static string _test = null;
        private static string _fixture = null;
        private static bool _debug = false;
        private static string _results = null;
        private static string _modelPath;
        private static bool _runDynamo = false;
        private const string _pluginGuid = "487f9ff0-5b34-4e7e-97bf-70fbff69194f";
        private const string _pluginClass = "Dynamo.Tests.DynamoTestFramework";
        private static string _workingDirectory;
        private static List<string> _journalPaths = new List<string>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            try
            {
                _workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (!ParseArguments(args))
                {
                    return;
                }

                if (!CreateJournalsForAssembly(_testAssembly, _fixture, _test))
                {
                    return;
                }

                if (File.Exists(_results))
                {
                    File.Delete(_results);
                }

                if (!File.Exists(_modelPath))
                {
                    Console.WriteLine("The specified test model does not exist");
                }

                foreach (var path in _journalPaths)
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "Revit.exe",
                        WorkingDirectory = _workingDirectory,
                        Arguments = path
                    };

                    var process = new Process {StartInfo = startInfo};
                    process.Start();
                    process.WaitForExit(120000);
                }

                Cleanup();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static bool ParseArguments(IEnumerable<string> args)
        {
            var showHelp = false;

            var p = new OptionSet()
            {
                {"dir=","The path to the working directory.", v=> _workingDirectory = Path.GetFullPath(v)},
                {"a=|assembly=", "The path to the test assembly.", v => _testAssembly = Path.GetFullPath(v)},
                {"r=|results=", "The path to the results file.", v=>_results = Path.GetFullPath(v)},
                {"f:|fixture:", "The full name (with namespace) of the test fixture.", v => _fixture = v},
                {"t:|testName:", "The name of a test to run", v => _test = v},
                {"d|debug", "Run in debug mode.", v=>_debug = v != null},
                {"h|help", "Show this message and exit.", v=> showHelp = v != null}
            };

            var notParsed = new List<string>();

            const string helpMessage = "Try 'DynamoTestFrameworkRunner --help' for more information.";
            
            try
            {
                notParsed = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(helpMessage);
                return false;
            }

            if (notParsed.Count > 0)
            {
                Console.WriteLine(string.Join(" ", notParsed.ToArray()));
                return false;
            }

            if (showHelp)
            {
                ShowHelp(p);
            }

            if (!File.Exists(_testAssembly))
            {
                Console.Write("The specified test assembly does not exist.");
                return false;
            }

            if (!string.IsNullOrEmpty(_workingDirectory) && !Directory.Exists(_workingDirectory))
            {
                Console.Write("The specified working directory does not exist.");
                return false;
            }

            return true;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: DynamoTestFrameworkRunner [OPTIONS]");
            Console.WriteLine("Run a test or a fixture of tests from an assembly.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static bool CreateJournalsForAssembly(string assemblyPath, string fixtureName="", string testName="")
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                
                // If a fixture is specified, then create 
                // journals for the fixture
                if (!string.IsNullOrEmpty(fixtureName))
                {
                    var fixtureType = assembly.GetType(fixtureName);
                    if (!CreateJournalsForFixture(fixtureType, testName))
                    {
                        Console.WriteLine(string.Format("Journals could not be created for {0}", _fixture));
                        return false;
                    }
                }
                // If no fixture is specified, but a test is specified, then
                // attempt to create a journal for that test.
                //else if (string.IsNullOrEmpty(fixtureName) && !string.IsNullOrEmpty(testName))
                //{
                //    // Attempt to find the test by method name alone
                //    var test = assembly.GetTypes().SelectMany(t => t.GetMethods()).FirstOrDefault(x => x.DeclaringType.Name + "." + x.Name == testName);
                //    if (test != null)
                //    {
                //        if (!CreateJournalsForFixture(test.DeclaringType, testName))
                //        {
                //            Console.WriteLine(string.Format("A journal could not be created for {0}", testName));
                //            return false;
                //        }
                //    }
                //}
                // If no fixture is specified, then create
                // journals for every fixture in the assembly
                else
                {
                    foreach (var fixtureType in assembly.GetTypes())
                    {
                        if (!CreateJournalsForFixture(fixtureType, testName))
                        {
                            Console.WriteLine(string.Format("Journals could not be created for {0}", fixtureType.Name));
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("The specified assembly could not be loaded for testing.");
                return false;
            }

            return true;
        }

        private static bool CreateJournalsForFixture(Type fixtureType, string testName="")
        {
            var fixtureAttribs = fixtureType.GetCustomAttributes(typeof (TestFixtureAttribute), true);
            if (!fixtureAttribs.Any())
            {
                Console.WriteLine("Specified fixture does not have the required TestFixture attribute.");
                return false;
            }

            if (!string.IsNullOrEmpty(testName))
            {
                var test = fixtureType.GetMethod(testName);
                var testAttribs = test.GetCustomAttributes(typeof(TestAttribute), false);
                if (!testAttribs.Any())
                {
                    Console.WriteLine("The specified test method does not have the Test attribute.");
                    return false;
                }

                var path = Path.Combine(_workingDirectory, testName + ".txt");

                if (!CreateJournalForTestInFixture(path, fixtureType, test))
                {
                    Console.WriteLine(string.Format("Journal could not be created for test:{0} in fixture:{1}", _test,
                            _fixture));
                    return false;
                }

                _journalPaths.Add(path);
            }
            else
            {
                foreach (var test in fixtureType.GetMethods())
                {
                    var testAttribs = test.GetCustomAttributes(typeof(TestAttribute), false);
                    if (!testAttribs.Any())
                    {
                        // skip this method
                        continue;
                    }

                    var path = Path.Combine(_workingDirectory, string.Format("{0}.{1}{2}",fixtureType.Name, test.Name,".txt"));

                    if (!CreateJournalForTestInFixture(path, fixtureType, test))
                    {
                        Console.WriteLine(string.Format("Journal could not be created for test:{0} in fixture:{1}", _test,
                            _fixture));
                        continue;
                    }

                    _journalPaths.Add(path);
                }
            }

            return true;
        }

        private static bool CreateJournalForTestInFixture(string path, Type fixtureType, MethodInfo test)
        {
            var testModelAttribs =  test.GetCustomAttributes(typeof(TestModelAttribute), false);
            if (!testModelAttribs.Any())
            {
                Console.WriteLine("The specified test does not have the required TestModelAttribute.");
                return false;
            }
            _modelPath = Path.Combine(_workingDirectory, ((TestModelAttribute)testModelAttribs[0]).Path);

            var runDynamoAttribs = test.GetCustomAttributes(typeof(RunDynamoAttribute),false);
            if (runDynamoAttribs.Any())
            {
                _runDynamo = ((RunDynamoAttribute)runDynamoAttribs[0]).RunDynamo;
            }

            using (var tw = new StreamWriter(path, false))
            {
                var journal = string.Format(@"'" +
                                        "Dim Jrn \n" +
                                        "Set Jrn = CrsJournalScript \n" +
                                        "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                        "Jrn.Data \"MRUFileName\"  , \"{0}\" \n" +
                                        "Jrn.RibbonEvent \"Execute external command:{1}:{2}\" \n" +
                                        "Jrn.Data \"APIStringStringMapJournalData\", 6, \"testName\", \"{3}\", \"fixtureName\", \"{4}\", \"testAssembly\", \"{5}\", \"resultsPath\", \"{6}\", \"runDynamo\",\"{7}\", \"debug\",\"{8}\" \n" +
                                        "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n" +
                                        "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"", 
                                        _modelPath, _pluginGuid, _pluginClass, _test, fixtureType.Name, _testAssembly, _results, _runDynamo, _debug);
                
                tw.Write(journal);
                tw.Flush();
            }

            return true;
        }

        private static void Cleanup()
        {
            foreach (var path in _journalPaths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            var journals = Directory.GetFiles(_workingDirectory, "journal.*.txt");
            foreach (var journal in journals)
            {
                File.Delete(journal);
            }
        }
    }
}
