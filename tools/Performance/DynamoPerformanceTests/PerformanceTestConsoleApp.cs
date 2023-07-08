using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using CommandLine;
using DynamoPerformanceTests.Enums;

namespace DynamoPerformanceTests
{
    public class Program
    {
        private enum ExitCode
        {
            ComparisonOK = 0,
            ComparisonFailure = 1
        }

        private static readonly string modelTestBaseReport = "DynamoPerformanceTests.DynamoModelPerformanceTestBase-report.csv";
        private static readonly string modelTestComparison = "DynamoPerformanceTests.Comparison-Model.csv";

        private static readonly string viewTestBaseReport = "DynamoPerformanceTests.DynamoViewPerformanceTestBase-report.csv";
        private static readonly string viewTestComparison = "DynamoPerformanceTests.Comparison-View.csv";

        public static void Main(string[] args)
        {
            // Default arguments
            IConfig config = PerformanceTestHelper.getFastReleaseConfig();

            Parser.Default
                .ParseArguments<PerformanceTestConsoleAppOptions>(args)
                .WithParsed(o =>
                    {
                        // Execute command
                        switch (o.Command)
                        {
                            case Command.NonOptimizedBenchmark:
                                config = PerformanceTestHelper.getDebugConfig();
                                goto case Command.Benchmark;

                            case Command.DebugInProcess:
                                config = PerformanceTestHelper.getDebugInProcessConfig();
                                goto case Command.Benchmark;

                            case Command.Benchmark:
                                DynamoViewPerformanceTestBase.testDirectory = o.TestDirectory;
                                var runSummaryWithUI = BenchmarkRunner.Run<DynamoViewPerformanceTestBase>(config);

                                goto case Command.ModelOnlyBenchmark;

                            case Command.ModelOnlyBenchmark:
                                DynamoModelPerformanceTestBase.testDirectory = o.TestDirectory;
                                var runSummaryWithoutUI = BenchmarkRunner.Run<DynamoModelPerformanceTestBase>(config);
                                break;

                            case Command.StandardConfigModelOnlyBenchmark:
                                config = PerformanceTestHelper.getReleaseConfig();
                                goto case Command.ModelOnlyBenchmark;

                            case Command.Compare:
                                Compare(o.BaseResultsPath, o.NewResultsPath, o.SaveComparisonPath);
                                break;
                        }
                    });
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
