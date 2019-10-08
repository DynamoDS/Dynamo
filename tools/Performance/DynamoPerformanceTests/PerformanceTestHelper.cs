using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Return the common library list to preload
        /// </summary>
        /// <returns>List of libs to preload</returns>
        public static List<string>  getDynamoDefaultLibs()
        {
            return new List<string>{
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DesignScriptBuiltin.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll",
                "GeometryColor.dll",
                "FFITarget.dll"
                };
        }

        /// <summary>
        /// Get the benchmark Dynamo release build config for performance test run
        /// </summary>
        /// <returns>Regular default Dynamo benchmark config</returns>
        public static DynamoBenchmarkConfig getReleaseConfig()
        {
            return new DynamoBenchmarkConfig();
        }

        /// <summary>
        /// Get the fast version of benchmark Dynamo release build config for performance test run
        /// </summary>
        /// <returns>Fast benchmark config</returns>
        public static FastBenchmarkReleaseConfig getFastReleaseConfig()
        {
            return new FastBenchmarkReleaseConfig();
        }

        /// <summary>
        /// Get the benchmark Dynamo debug build config for performance test run
        /// </summary>
        /// <returns>Benchmark config for debug build</returns>
        public static BenchmarkDebugConfig getDebugConfig()
        {
            return new BenchmarkDebugConfig();
        }

        /// <summary>
        /// Get the benchmark debug in process Dynamo debug build config for performance test run
        /// </summary>
        /// <returns>Benchmark config for inproces debugging</returns>
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

        /// <summary>
        /// Gets the absolute path for a path relative to the executing assembly location
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFullPath(string path)
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string dir = fi.DirectoryName;
            string f = Path.Combine(dir, path);
            return Path.GetFullPath(f);
        }
    }
}
