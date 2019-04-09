using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Dynamo;
using NDesk.Options;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// Class utilizing BenchmarkDotNet to implement a performance
    /// benchmarking framework. This class currently measures Open and Run times
    /// for DYN graphs in Dynamo.
    /// </summary>
    public class PerformanceTestFramework : DynamoModelTestBase
    {
        // Config class that when initialized and used to run the benchmarks
        // allows for testing of debug versions of DynamoCore targets.
        public class AllowNonOptimized : ManualConfig
        {
            public AllowNonOptimized(string testDir)
            {
                Add(JitOptimizationsValidator.DontFailOnError);

                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

                testDirectory = testDir;
            }
        }

        // Config class used to pass command line arguments from the 
        // benchmark runner to all benchmarks defined in the test framework class.
        public class BenchmarkConfig : ManualConfig
        {
            public BenchmarkConfig(string testDir)
            {
                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default

                var defaultColumns = DefaultConfig.Instance.GetColumnProviders().ToList();
                defaultColumns.RemoveAt(3); // Remove DynamoFilePath column
                Add(defaultColumns.ToArray());

                Add(new GraphNameColumn()); // Add Graph Name column

                testDirectory = testDir;
            }
        }

        private static string testDirectory;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            base.GetLibrariesToPreload(libraries);

            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DynamoConversions.dll");
            libraries.Add("DynamoUnits.dll");
            libraries.Add("Tessellation.dll");
            libraries.Add("Analysis.dll");
            libraries.Add("GeometryColor.dll");
            libraries.Add("FFITarget.dll");
        }

        /// <summary>
        /// Automated creation of performance test cases, one for each
        /// parameter source.
        /// </summary>
        [ParamsSource(nameof(PerformanceTestSource))]
        public string DynamoFilePath { get; set; }

        #region Iteration setup and cleanup methods for Benchmarks

        /// <summary>
        /// Setup method to be called before each OpenModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Open))]
        public void IterationSetupOpenModel()
        {
            Setup();
        }

        /// <summary>
        /// Setup method to be called before each RunModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Run))]
        public void IterationSetupRunModel()
        {
            Setup();
            
            //open the dyn file
            OpenModel(DynamoFilePath);
        }

        /// <summary>
        /// Cleanup method to be called after each benchmark.
        /// </summary>
        [IterationCleanup]
        public void IterationCleanup()
        {
            Cleanup();
        }

        #endregion

        #region Benchmark methods

        [Benchmark]
        public void Open()
        {
            //open the dyn file
            OpenModel(DynamoFilePath);
        }

        [Benchmark]
        public void Run()
        {
            BeginRun();
        }

        #endregion

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> PerformanceTestSource()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string dir = fi.DirectoryName;

            // Test location for all DYN files to be measured for performance 
            string testsLoc = Path.Combine(dir, testDirectory);
            var regTestPath = Path.GetFullPath(testsLoc);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                yield return fileInfo.FullName;
            }
        }
    }


    public class Program
    {
        private enum Command
        {
            Benchmark,
            Debug,
            Compare,
        }

        public static void Main(string[] args)
        {
            var showHelp = false;

            if (args.Length <= 0)
            {
                Console.WriteLine("Please specify a command.");
                return;
            }
            
            // Default arguments
            var testDirectory = "../../../graphs";
            var baseResultsPath = "BenchmarkDotNet.Artifacts/results/DynamoPerformanceTests.PerformanceTestFramework-report.csv";
            var newResultsPath = string.Empty;
            var saveComparisonPath = string.Empty;
            
            // Command line options
            var opts = new OptionSet() {
                { "g=|graphs=", "Path to Directory containing test graphs. Defaults to 'Dynamo/tools/Performance/DynamoPerformanceTests/graphs'", v => { testDirectory = v; } },
                { "b=|base=", "Path to performance results file to use as comparison base. Defaults to 'BenchmarkDotNet.Artifacts/results/DynamoPerformanceTests.PerformanceTestFramework-report.csv'", v => { baseResultsPath = v; }},
                { "n=|new=", "Path to new performance results file to compare against the baseline", v => { newResultsPath = v; }},
                { "s=|save=", "Location to save comparison csv", v => { saveComparisonPath = v; }},
                { "h|help",  "show this message and return", v => showHelp = v != null },
            };
            opts.Parse(args);

            // Show help
            if (showHelp)
            {
                ShowHelp(opts);
                return;
            }

            // Get command
            Command command;
            var commandRecognized = Enum.TryParse(args[0], out command);
            if (!commandRecognized)
            {
                Console.WriteLine("Command \"{0}\" not recognized.", args[0]);
                return;
            }

            // Execute command
            switch (command)
            {
                case Command.Benchmark:
                    var summary = BenchmarkRunner.Run<PerformanceTestFramework>(
                        new PerformanceTestFramework.BenchmarkConfig(testDirectory));
                    break;

                case Command.Debug:
                    summary = BenchmarkRunner.Run<PerformanceTestFramework>(
                        new PerformanceTestFramework.AllowNonOptimized(testDirectory));
                    break;

                case Command.Compare:
                    if (baseResultsPath == string.Empty)
                    {
                        Console.WriteLine("Please generate a baseline benchmark results file," +
                            "or provide a path for a baseline benchmark results file.");
                        break;
                    }
                    if (newResultsPath == string.Empty)
                    {
                        Console.WriteLine("Please provide a path to a benchmark results file to compare against the baseline.");
                        break;
                    }

                    // Create comparer
                    var comparer = new ResultsComparer(baseResultsPath, newResultsPath);
                    if (saveComparisonPath != string.Empty)
                    {
                        comparer.WriteResultsToCSV(saveComparisonPath);
                    }
                    break;

                default:
                    break;
            }
        }

        private static void ShowHelp(OptionSet opSet)
        {
            Console.WriteLine("\ncommands:");
            Console.WriteLine("  Benchmark: Run performance tests");
            Console.WriteLine("  Debug: Use this call in order to run benchmarks on debug build of DynamoCore");
            Console.WriteLine("  Compare: Compare results from two performance test runs");
            Console.WriteLine("\noptions:");
            opSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
