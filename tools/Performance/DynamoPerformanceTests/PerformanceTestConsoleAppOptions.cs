using CommandLine;
using DynamoPerformanceTests.Enums;

namespace DynamoPerformanceTests
{
    internal class PerformanceTestConsoleAppOptions
    {
        const string GraphsHelpText = "Path to Directory containing test graphs. Defaults to 'Dynamo/tools/Performance/DynamoPerformanceTests/graphs/'";
        const string BaseHelpText = "Path to Directory containing performance results files to use as comparison base. Defaults to 'BenchmarkDotNet.Artifacts/results/'";
        const string NewHelpText = "Path to Directory containing new performance results files to compare against the baseline";
        const string SaveHelpText = "Location to save comparison csv";
        const string CommandHelpText = "\nBenchmark: Run performance tests\n" +
            "NonOptimizedBenchmark: Use this call in order to run benchmarks on debug build of DynamoCore\n" +
            "DebugInProcess: Use this call to get debug in process config in order to debug benchmarks\n" +
            "Compare: Compare results from two performance test runs";

        [Option('g', "graphs", Required = false, HelpText = GraphsHelpText, Default = "../../../graphs/")]
        public string TestDirectory { get; set; }

        [Option('b', "base", Required = false, HelpText = BaseHelpText, Default = "")]
        public string BaseResultsPath { get; set; }

        [Option('n', "new", Required = false, HelpText = NewHelpText, Default = "BenchmarkDotNet.Artifacts/results/")]
        public string NewResultsPath { get; set; }

        [Option('s', "save", Required = false, HelpText = SaveHelpText, Default = "")]
        public string SaveComparisonPath { get; set; }

        [Option('c', "command", Required = true, HelpText = CommandHelpText, Default = "")]
        public Command Command { get; set; }
    }
}
