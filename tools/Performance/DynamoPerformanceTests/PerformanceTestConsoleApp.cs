using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NDesk.Options;
using System.Linq;

namespace DynamoPerformanceTests
{
    public class Program
    {
        private enum ExitCode
        {
            ComparisonOK = 0,
            ComparisonFailure = 1
        }

        private enum Command
        {
            Benchmark,
            NonOptimizedBenchmark,
            DebugInProcess,
            Compare,
            ModelOnlyBenchmark,
            StandardConfigModelOnlyBenchmark
        }

        private static readonly string modelTestBaseReport = "DynamoPerformanceTests.DynamoModelPerformanceTestBase-report.csv";
        private static readonly string modelTestComparison = "DynamoPerformanceTests.Comparison-Model.csv";

        private static readonly string viewTestBaseReport = "DynamoPerformanceTests.DynamoViewPerformanceTestBase-report.csv";
        private static readonly string viewTestComparison = "DynamoPerformanceTests.Comparison-View.csv";

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

            ResultsComparer modelComparer = null;
            ResultsComparer viewComparer = null;

            try
            {

                // Create Model comparer
                Console.WriteLine("\nComparison of Model tests: \n");
                var baseModelPath = Path.Combine(PerformanceTestHelper.GetFullPath(baseResultsPath), modelTestBaseReport  );
                var newModelPath = Path.Combine(PerformanceTestHelper.GetFullPath(newResultsPath),modelTestBaseReport);
                var modelSavePath = Path.Combine(PerformanceTestHelper.GetFullPath(savePath),modelTestComparison );

                modelComparer = new ResultsComparer(baseModelPath, newModelPath, modelSavePath);

                // Create View comparer
                Console.WriteLine("\nComparison of View tests: \n");
                var baseViewPath = Path.Combine(PerformanceTestHelper.GetFullPath(baseResultsPath),viewTestBaseReport );
                var newViewPath = Path.Combine(PerformanceTestHelper.GetFullPath(newResultsPath), viewTestBaseReport);
                var viewSavePath = Path.Combine(PerformanceTestHelper.GetFullPath(savePath),viewTestComparison );

                viewComparer = new ResultsComparer(baseViewPath, newViewPath, viewSavePath);
            }
            //catch here - likely we could not create a view comparer if view comparison did not run.
            catch(Exception e)
            {
                Console.WriteLine($"exception while comparing results {e} {Environment.NewLine} {e.Message}");

            }

            //return result of comparison
            if (modelComparer != null)
            {
                if(modelComparer.ComparisonData.Any(x=>x.ResultState == ResultsComparer.ComparisonResultState.FAIL))
                {
                    Console.WriteLine("Comparison failed, some model performance benchmarks failed. Please see log above for details.");
                    Environment.Exit((int)ExitCode.ComparisonFailure );
                }
            }

            if (viewComparer != null)
            {
                if (viewComparer.ComparisonData.Any(x => x.ResultState == ResultsComparer.ComparisonResultState.FAIL))
                {
                    Console.WriteLine("Comparison failed, some view performance benchmarks failed. Please see log above for details.");
                    Environment.Exit((int)ExitCode.ComparisonFailure);
                }
            }

        }
    }
}
