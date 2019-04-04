using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// Use this static class as Global helper to get all the prep data
    /// </summary>
    public static class PerformanceTestHelper
    {
        /// <summary>
        /// Get the benchmark release config for performance test run
        /// </summary>
        /// <returns></returns>
        public static BenchmarkReleaseConfig getReleaseConfig()
        {
            return new BenchmarkReleaseConfig();
        }

        /// <summary>
        /// Get the benchmark debug config for performance test run
        /// </summary>
        /// <returns></returns>
        public static BenchmarkDebugConfig getDebugConfig()
        {
            return new BenchmarkDebugConfig();
        }

        /// <summary>
        /// Config class that when initialized and used to run the benchmarks
        /// allows for testing of debug versions of DynamoCore targets.
        /// </summary>
        public class BenchmarkDebugConfig : ManualConfig
        {
            public BenchmarkDebugConfig()
            {
                Add(JitOptimizationsValidator.DontFailOnError);

                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                var defaultColumns = DefaultConfig.Instance.GetColumnProviders().ToList();
                defaultColumns.RemoveAt(3); // Remove DynamoFilePath column
                Add(defaultColumns.ToArray());

                Add(new GraphNameColumn()); // Add Graph Name column
            }
        }

        /// <summary>
        /// Config class used to pass command line arguments from the 
        /// benchmark runner to all benchmarks defined in the test framework class.
        /// </summary>
        public class BenchmarkReleaseConfig : ManualConfig
        {
            public BenchmarkReleaseConfig()
            {
                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default

                var defaultColumns = DefaultConfig.Instance.GetColumnProviders().ToList();
                defaultColumns.RemoveAt(3); // Remove DynamoFilePath column
                Add(defaultColumns.ToArray());

                Add(new GraphNameColumn()); // Add Graph Name column
            }
        }
    }
}
