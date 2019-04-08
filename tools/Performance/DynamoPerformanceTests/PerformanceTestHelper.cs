using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// Use this static class as Global helper to get various benchmark config
    /// </summary>
    public static class PerformanceTestHelper
    {
        /// <summary>
        /// Get the benchmark Dynamo release build config for performance test run
        /// </summary>
        /// <returns></returns>
        public static BenchmarkReleaseConfig getReleaseConfig()
        {
            return new BenchmarkReleaseConfig();
        }

        /// <summary>
        /// Get the fast version of benchmark Dynamo release build config for performance test run
        /// </summary>
        /// <returns></returns>
        public static FastBenchmarkReleaseConfig getFastReleaseConfig()
        {
            return new FastBenchmarkReleaseConfig();
        }

        /// <summary>
        /// Get the benchmark Dynamo debug build config for performance test run
        /// </summary>
        /// <returns></returns>
        public static BenchmarkDebugConfig getDebugConfig()
        {
            return new BenchmarkDebugConfig();
        }

        /// <summary>
        /// Get the benchmark debug in process Dynamo debug build config for performance test run
        /// </summary>
        /// <returns></returns>
        public static DebugInProcessConfig getDebugInProcessConfig()
        {
            return new DebugInProcessConfig();
        }

        /// <summary>
        /// Base config class for regular benchmark job
        /// </summary>
        public class DynamoBenchmarkConfig : ManualConfig
        {
            /// <summary>
            /// Minimum count of warmup iterations that should be performed
            /// </summary>
            protected int DynamoMinWarmupCount = 6;

            /// <summary>
            /// Maximum count of warmup iterations that should be performed
            /// </summary>
            protected int DynamoMaxWarmuoCount = 9;

            /// <summary>
            /// Benchmark process will be launched only once
            /// </summary>
            protected int DynamoLaunchCount = 1;

            /// <summary>
            /// Minimum count of target iterations that should be performed.
            /// </summary>
            protected int DynamoMinIterationCount = 6;

            /// <summary>
            /// Maximum count of target iterations that should be performed.
            /// </summary>
            protected int DynamoMaxIterationCount = 9;

            public DynamoBenchmarkConfig()
            {
                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default

                var defaultColumns = DefaultConfig.Instance.GetColumnProviders().ToList();
                defaultColumns.RemoveAt(3); // Remove DynamoFilePath column
                Add(defaultColumns.ToArray());
                Add(new GraphNameColumn()); // Add Graph Name column
            }
        }

        /// <summary>
        /// Config class that when initialized and used to run the benchmarks
        /// allows for testing of debug versions of DynamoCore targets.
        /// </summary>
        public class BenchmarkDebugConfig : DynamoBenchmarkConfig
        {
            public BenchmarkDebugConfig() : base()
            {
                Add(JitOptimizationsValidator.DontFailOnError);
            }
        }

        /// <summary>
        /// Config class used to pass command line arguments from the 
        /// benchmark runner to all benchmarks defined in the test framework class.
        /// </summary>
        public class BenchmarkReleaseConfig : DynamoBenchmarkConfig
        {
            public BenchmarkReleaseConfig() : base()
            {
            }
        }

        /// <summary>
        /// A faster version of Config class than default used to pass command line arguments from the 
        /// benchmark runner to all benchmarks defined in the test framework class.
        /// </summary>
        public class FastBenchmarkReleaseConfig : DynamoBenchmarkConfig
        {
            public FastBenchmarkReleaseConfig() : base()
            {
                Add(Job.Default
                    .WithMinWarmupCount(DynamoMinWarmupCount)
                    .WithMaxWarmupCount(DynamoMaxWarmuoCount)
                    .WithLaunchCount(DynamoLaunchCount)
                    .WithMinIterationCount(DynamoMinIterationCount)
                    .WithMaxIterationCount(DynamoMaxIterationCount)
                );
            }
        }
    }
}
