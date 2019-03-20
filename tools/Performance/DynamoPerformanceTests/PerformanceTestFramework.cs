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
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

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
        [IterationSetup(Target = nameof(OpenModelBenchmark))]
        public void IterationSetupOpenModel()
        {
            Setup();
        }

        /// <summary>
        /// Setup method to be called before each RunModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(RunModelBenchmark))]
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
        public void OpenModelBenchmark()
        {
            //open the dyn file
            OpenModel(DynamoFilePath);
        }

        [Benchmark]
        public void RunModelBenchmark()
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
        public static void Main(string[] args)
        {
            // Running with an input dir location:
            // DynamoPerformanceTests.exe "C:\directory path\"
            // 
            if (args.Length <= 0)
            {
                Console.WriteLine("Supply a path to a test directory containing DYN files");
            }

            // Use this call in order to run benchmarks on debug build of DynamoCore
            //var summary = BenchmarkRunner.Run<PerformanceTestFramework>(
            //new PerformanceTestFramework.AllowNonOptimized(testDir));

            var summary = BenchmarkRunner.Run<PerformanceTestFramework>(
                new PerformanceTestFramework.BenchmarkConfig(args[0]));
        }
    }
}
