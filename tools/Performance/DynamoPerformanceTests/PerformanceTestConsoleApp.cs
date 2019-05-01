using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NDesk.Options;

namespace DynamoPerformanceTests
{
    public class Program
    {
        private enum Command
        {
            Benchmark,
            NonOptimizedBenchmark,
            DebugInProcess,
            Compare,
            ModelOnlyBenchmark,
            StandardConfigModelOnlyBenchmark
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
            IConfig config = PerformanceTestHelper.getFastReleaseConfig();
            var testDirectory = "../../../graphs/";
            var baseResultsPath = string.Empty;
            var newResultsPath = "BenchmarkDotNet.Artifacts/results/";
            var saveComparisonPath = string.Empty;

            // Command line options
            var opts = new OptionSet() {
                { "g=|graphs=", "Path to Directory containing test graphs. Defaults to 'Dynamo/tools/Performance/DynamoPerformanceTests/graphs/'", v => { testDirectory = v; } },
                { "b=|base=", "Path to Directory containing performance results files to use as comparison base. Defaults to 'BenchmarkDotNet.Artifacts/results/'", v => { baseResultsPath = v; }},
                { "n=|new=", "Path to Directory containing new performance results files to compare against the baseline", v => { newResultsPath = v; }},
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
                case Command.NonOptimizedBenchmark:
                    config = PerformanceTestHelper.getDebugConfig();
                    goto case Command.Benchmark;

                case Command.DebugInProcess:
                    config = PerformanceTestHelper.getDebugInProcessConfig();
                    goto case Command.Benchmark;

                case Command.Benchmark:
                    DynamoViewPerformanceTestBase.testDirectory = testDirectory;
                    var runSummaryWithUI = BenchmarkRunner.Run<DynamoViewPerformanceTestBase>(config);

                    goto case Command.ModelOnlyBenchmark;

                case Command.ModelOnlyBenchmark:
                    DynamoModelPerformanceTestBase.testDirectory = testDirectory;
                    var runSummaryWithoutUI = BenchmarkRunner.Run<DynamoModelPerformanceTestBase>(config);
                    break;

                case Command.StandardConfigModelOnlyBenchmark:
                    config = PerformanceTestHelper.getReleaseConfig();
                    goto case Command.ModelOnlyBenchmark;

                case Command.Compare:
                    Compare(baseResultsPath, newResultsPath, saveComparisonPath);
                    break;
            }
        }

        private static void ShowHelp(OptionSet opSet)
        {
            Console.WriteLine("\ncommands:");
            Console.WriteLine("  Benchmark: Run performance tests");
            Console.WriteLine("  NonOptimizedBenchmark: Use this call in order to run benchmarks on debug build of DynamoCore");
            Console.WriteLine("  DebugInProcess: Use this call to get debug in process config in order to debug benchmarks");
            Console.WriteLine("  Compare: Compare results from two performance test runs");
            Console.WriteLine("\noptions:");
            opSet.WriteOptionDescriptions(Console.Out);
        }

        private static void Compare(string baseResultsPath, string newResultsPath, string savePath)
        {
            if (baseResultsPath == string.Empty)
            {
                Console.WriteLine("Please generate a baseline benchmark results file," +
                    "or provide a path for a baseline benchmark results file.");
                return;
            }
            if (newResultsPath == string.Empty)
            {
                Console.WriteLine("Please provide a path to a benchmark results file to compare against the baseline.");
                return;
            }

            // Create Model comparer
            Console.WriteLine("\nComparison of Model tests: \n");
            var baseModelPath = Path.Combine(PerformanceTestHelper.GetFullPath(baseResultsPath), "DynamoPerformanceTests.DynamoModelPerformanceTestBase-report.csv");
            var newModelPath = Path.Combine(PerformanceTestHelper.GetFullPath(newResultsPath), "DynamoPerformanceTests.DynamoModelPerformanceTestBase-report.csv");
            var modelSavePath = Path.Combine(PerformanceTestHelper.GetFullPath(savePath), "DynamoPerformanceTests.Comparison-Model.csv");
            var modelComparer = new ResultsComparer(baseModelPath, newModelPath, modelSavePath);

            // Create View comparer
            Console.WriteLine("\nComparison of View tests: \n");
            var baseViewPath = Path.Combine(PerformanceTestHelper.GetFullPath(baseResultsPath), "DynamoPerformanceTests.DynamoViewPerformanceTestBase-report.csv");
            var newViewPath = Path.Combine(PerformanceTestHelper.GetFullPath(newResultsPath), "DynamoPerformanceTests.DynamoViewPerformanceTestBase-report.csv");
            var viewSavePath = Path.Combine(PerformanceTestHelper.GetFullPath(savePath), "DynamoPerformanceTests.Comparison-View.csv");
            var viewComparer = new ResultsComparer(baseViewPath, newViewPath, viewSavePath);
        }
    }
}
