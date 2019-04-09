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
            Debug,
            InProcess,
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
            IConfig config = PerformanceTestHelper.getFastReleaseConfig();
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
                case Command.Debug:
                    config = PerformanceTestHelper.getDebugConfig();
                    goto case Command.Benchmark;

                case Command.InProcess:
                    config = PerformanceTestHelper.getDebugInProcessConfig();
                    goto case Command.Benchmark;

                case Command.Benchmark:
                    DynamoViewPerformanceTestBase.testDirectory = testDirectory;
                    var runSummaryWithUI = BenchmarkRunner.Run<DynamoViewPerformanceTestBase>(config);

                    DynamoModelPerformanceTestBase.testDirectory = testDirectory;
                    var runSummaryWithoutUI = BenchmarkRunner.Run<DynamoModelPerformanceTestBase>(config);
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
            Console.WriteLine("  InProcess: Use this call to get debug in process config in order to debug benchmarks");
            Console.WriteLine("  Compare: Compare results from two performance test runs");
            Console.WriteLine("\noptions:");
            opSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
