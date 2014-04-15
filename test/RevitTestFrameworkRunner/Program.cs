using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.RevitAddIns;
using Dynamo.Tests;
using Dynamo.Utilities;
using NDesk.Options;
using NUnit.Framework;

namespace RevitTestFrameworkRunner
{
    class Program
    {
        internal static string _testAssembly = null;
        internal static string _test = null;
        internal static string _fixture = null;
        internal static bool _isDebug = false;
        internal static string _results = null;
        internal const string _pluginGuid = "487f9ff0-5b34-4e7e-97bf-70fbff69194f";
        internal const string _pluginClass = "Dynamo.Tests.RevitTestFramework";
        internal static string _workingDirectory;
        internal static bool _gui = true;
        internal static string _revitPath;
        internal static List<string> _journalPaths = new List<string>();
        internal static int _runCount = 0;

        public static event EventHandler TestRunsComplete;
        private static void OnTestRunsComplete()
        {
            if (TestRunsComplete != null)
            {
                TestRunsComplete(null, EventArgs.Empty);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            try
            {
                TestRunsComplete += Program_TestRunsComplete;

                if (!ParseArguments(args))
                {
                    return;
                }

                var vm = new ViewModel();

                if (!FindRevit(vm.Products))
                {
                    return;
                }
                
                if (_gui)
                {
                    LoadSettings();

                    if (!string.IsNullOrEmpty(_testAssembly) && File.Exists(_testAssembly))
                    {
                        Refresh(vm);
                    }

                    // Show the user interface
                    var view = new View(vm);
                    view.ShowDialog();

                    SaveSettings();
                }
                else
                {
                    if (string.IsNullOrEmpty(_workingDirectory))
                    {
                        _workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    }

                    // In any case here, the test assembly cannot be null
                    if (string.IsNullOrEmpty(_testAssembly))
                    {
                        Console.WriteLine("You must specify at least a test assembly.");
                        return;
                    }

                    if (!ReadAssembly(_testAssembly, vm.Assemblies))
                    {
                        return;
                    }

                    if (File.Exists(_results))
                    {
                        File.Delete(_results);
                    }

                    // If fixture name and test name are specified
                    if (string.IsNullOrEmpty(_fixture) && string.IsNullOrEmpty(_test))
                    {
                        _runCount = vm.Assemblies.SelectMany(a => a.Fixtures.SelectMany(f => f.Tests)).Count();
                        foreach (var ad in vm.Assemblies)
                        {
                            RunAssembly(ad);
                        }
                    }
                    // If test is not specified but fixture is specified
                    else if (string.IsNullOrEmpty(_test) && !string.IsNullOrEmpty(_fixture))
                    {
                        var fd = vm.Assemblies.SelectMany(x => x.Fixtures).FirstOrDefault(f => f.Name == _fixture);
                        if (fd != null)
                        {
                            _runCount = fd.Tests.Count;
                            RunFixture(fd);
                        }
                    }
                    // If test is specified
                    else if (string.IsNullOrEmpty(_fixture) && !string.IsNullOrEmpty(_test))
                    {
                        var td =
                            vm.Assemblies.SelectMany(a => a.Fixtures.SelectMany(f => f.Tests))
                                .FirstOrDefault(t => t.Name == _test);
                        if (td != null)
                        {
                            _runCount = 1;
                            RunTest(td);
                        }
                    }
                }

                Cleanup();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Program_TestRunsComplete(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_results) && 
                File.Exists(_results) &&
                _gui)
            {
                Process.Start(_results);
            }
        }

        private static bool FindRevit(IList<RevitProduct> productList)
        {
            var products = RevitProductUtility.GetAllInstalledRevitProducts();
            if (!products.Any())
            {
                Console.WriteLine("No versions of revit could be found");
                return false;
            }

            products.ForEach(productList.Add);
            return true;
        }

        private static void SaveSettings()
        {
            Properties.Settings.Default.workingDirectory = _workingDirectory;
            Properties.Settings.Default.assemblyPath = _testAssembly;
            Properties.Settings.Default.resultsPath = _results;
            Properties.Settings.Default.isDebug = _isDebug;
            Properties.Settings.Default.Save();
        }

        private static void LoadSettings()
        {
            _workingDirectory = !string.IsNullOrEmpty(Properties.Settings.Default.workingDirectory)
                ? Properties.Settings.Default.workingDirectory
                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _testAssembly = !string.IsNullOrEmpty(Properties.Settings.Default.assemblyPath)
                ? Properties.Settings.Default.assemblyPath
                : null;

            _results = !string.IsNullOrEmpty(Properties.Settings.Default.resultsPath)
                ? Properties.Settings.Default.resultsPath
                : null;

            _isDebug = Properties.Settings.Default.isDebug;
        }

        private static bool ParseArguments(IEnumerable<string> args)
        {
            var showHelp = false;

            var p = new OptionSet()
            {
                {"dir:","The path to the working directory.", v=> _workingDirectory = Path.GetFullPath(v)},
                {"a:|assembly:", "The path to the test assembly.", v => _testAssembly = Path.GetFullPath(v)},
                {"r:|results:", "The path to the results file.", v=>_results = Path.GetFullPath(v)},
                {"f:|fixture:", "The full name (with namespace) of the test fixture.", v => _fixture = v},
                {"t:|testName:", "The name of a test to run", v => _test = v},
                {"gui:", "Show the revit test runner gui.", v=>_gui = v != null},
                {"d|debug", "Run in debug mode.", v=>_isDebug = v != null},
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
                return false;
            }

            if (!string.IsNullOrEmpty(_testAssembly) && !File.Exists(_testAssembly))
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

        private static bool ReadAssembly(string assemblyPath, IList<IAssemblyData> data)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);

                var assData = new AssemblyData(assemblyPath, assembly.GetName().Name);
                data.Add(assData);

                foreach (var fixtureType in assembly.GetTypes())
                {
                    if (!ReadFixture(fixtureType, assData))
                    {
                        Console.WriteLine(string.Format("Journals could not be created for {0}", fixtureType.Name));
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

        private static bool ReadFixture(Type fixtureType, IAssemblyData data)
        {
            var fixtureAttribs = fixtureType.GetCustomAttributes(typeof (TestFixtureAttribute), true);
            if (!fixtureAttribs.Any())
            {
                Console.WriteLine("Specified fixture does not have the required TestFixture attribute.");
                return false;
            }

            var fixData = new FixtureData(data, fixtureType.Name);
            data.Fixtures.Add(fixData);

            foreach (var test in fixtureType.GetMethods())
            {
                var testAttribs = test.GetCustomAttributes(typeof(TestAttribute), false);
                if (!testAttribs.Any())
                {
                    // skip this method
                    continue;
                }

                if (!ReadTest(test, fixData))
                {
                    Console.WriteLine(string.Format("Journal could not be created for test:{0} in fixture:{1}", _test,
                        _fixture));
                    continue;
                }
            }

            return true;
        }

        private static bool ReadTest(MethodInfo test, IFixtureData data)
        {
            var testModelAttribs =  test.GetCustomAttributes(typeof(TestModelAttribute), false);
            if (!testModelAttribs.Any())
            {
                Console.WriteLine("The specified test does not have the required TestModelAttribute.");
                return false;
            }
            
            var modelPath = Path.GetFullPath(Path.Combine(_workingDirectory, ((TestModelAttribute)testModelAttribs[0]).Path));

            var runDynamoAttribs = test.GetCustomAttributes(typeof(RunDynamoAttribute),false);
            var runDynamo = false;
            if (runDynamoAttribs.Any())
            {
                runDynamo = ((RunDynamoAttribute)runDynamoAttribs[0]).RunDynamo;
            }

            var testData = new TestData(data, test.Name, modelPath, runDynamo);
            data.Tests.Add(testData);

            return true;
        }

        private static void CreateJournal(string path, string testName, string fixtureName, string assemblyPath, string resultsPath, string modelPath)
        {
            using (var tw = new StreamWriter(path, false))
            {
                var journal = string.Format(@"'" +
                                            "Dim Jrn \n" +
                                            "Set Jrn = CrsJournalScript \n" +
                                            "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\"  , \"{0}\" \n" +
                                            "Jrn.RibbonEvent \"Execute external command:{1}:{2}\" \n" +
                                            "Jrn.Data \"APIStringStringMapJournalData\", 5, \"testName\", \"{3}\", \"fixtureName\", \"{4}\", \"testAssembly\", \"{5}\", \"resultsPath\", \"{6}\", \"debug\",\"{7}\" \n" +
                                            "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n" +
                                            "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"",
                    modelPath, _pluginGuid, _pluginClass, testName, fixtureName, assemblyPath, resultsPath, _isDebug);

                tw.Write(journal);
                tw.Flush();

                _journalPaths.Add(path);
            }
        }

        public static void Refresh(ViewModel vm)
        {
            vm.Assemblies.Clear();
            ReadAssembly(_testAssembly, vm.Assemblies);
        }

        public static void RunAssembly(IAssemblyData ad)
        {
            foreach (var fix in ad.Fixtures)
            {
                RunFixture(fix);
            }
        }

        public static void RunFixture(IFixtureData fd)
        {
            foreach (var td in fd.Tests)
            {
                RunTest(td);
            }
        }

        public static void RunTest(ITestData td)
        {
            var path = Path.Combine(_workingDirectory, td.Name + ".txt");

            CreateJournal(path, td.Name, td.Fixture.Name, td.Fixture.Assembly.Path, _results, td.ModelPath);

            var startInfo = new ProcessStartInfo()
            {
                FileName = _revitPath,
                WorkingDirectory = _workingDirectory,
                Arguments = path
            };

            Console.WriteLine("Running {0}", path);
            var process = new Process { StartInfo = startInfo };
            process.Start();

            if (_isDebug)
            {
                process.WaitForExit();
            }  
            else
            {
                if (!process.WaitForExit(120000))
                {
                    process.Kill();
                    System.Threading.Thread.Sleep(10000);
                }
            }

            _runCount --;
            if (_runCount == 0)
            {
                OnTestRunsComplete();
            }
        }

        internal static void Cleanup()
        {
            try
            {
                foreach (var path in _journalPaths)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                _journalPaths.Clear();

                var journals = Directory.GetFiles(_workingDirectory, "journal.*.txt");
                foreach (var journal in journals)
                {
                    File.Delete(journal);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("One or more journal files could not be deleted.");
            }
        }
    }

    internal class AssemblyData : IAssemblyData
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public IList<IFixtureData> Fixtures { get; set; }

        public AssemblyData(string path, string name)
        {
            Fixtures = new List<IFixtureData>();
            Path = path;
            Name = name;
        }
    }

    internal class FixtureData : IFixtureData
    {
        public string Name { get; set; }
        public IList<ITestData> Tests { get; set; }
        public IAssemblyData Assembly { get; set; }
        public FixtureData(IAssemblyData assembly, string name)
        {
            Assembly = assembly;
            Tests = new List<ITestData>();
            Name = name;
        }
    }

    internal class TestData : ITestData
    {
        public string Name { get; set; }
        public bool RunDynamo { get; set; }
        public string ModelPath { get; set; }
        public IFixtureData Fixture { get; set; }

        public TestData(IFixtureData fixture, string name, string modelPath, bool runDynamo)
        {
            Fixture = fixture;
            Name = name;
            ModelPath = modelPath;
            RunDynamo = runDynamo;
        }
    }
}
