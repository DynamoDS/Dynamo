using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Dynamo;
using Dynamo.Tests;

namespace DynamoPerformanceTests
{
    public class PerformanceTestBase : IPerformanceTest
    {
        /// <summary>
        /// Test directory containing the graph suite which the current performance test will be running against
        /// </summary>
        private static string testDirectory;

        /// <summary>
        /// Automated creation of performance test cases, one for each
        /// parameter source.
        /// </summary>
        [ParamsSource(nameof(PerformanceTestSource))]
        public string DynamoFilePath { get; set; }

        /// <summary>
        /// Config class that when initialized and used to run the benchmarks
        /// allows for testing of debug versions of DynamoCore targets.
        /// </summary>
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

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> PerformanceTestSource()
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

    public class DynamoViewModelPerformanceTestBase : DynamoViewModelUnitTest, IPerformanceTest
    {
        private PerformanceTestBase testBase;

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

        #region Iteration setup and cleanup methods for Benchmarks
        /// <summary>
        /// Setup method to be called before each OpenModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(OpenModelBenchmark))]
        public void IterationSetupOpenModelWithUI()
        {
            Setup();
        }
        
        /// <summary>
         /// Setup method to be called before each RunModel benchmark.
         /// </summary>
        [IterationSetup(Target = nameof(RunModelBenchmark))]
        public void IterationSetupRunModelWithUI()
        {
            Setup();

            //open the dyn file
            OpenModel(testBase.DynamoFilePath);
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
            OpenModel(testBase.DynamoFilePath);
        }

        [Benchmark]
        public void RunModelBenchmark()
        {
            BeginRun();
        }

        #endregion

        public IEnumerable<string> PerformanceTestSource()
        {
            return testBase.PerformanceTestSource();
        }
    }

    /// <summary>
    /// Class utilizing BenchmarkDotNet to implement a performance
    /// benchmarking framework. This class currently measures Open and Run times
    /// for DYN graphs in Dynamo.
    /// </summary>
    public class DynamoModelPerformanceTestBase : DynamoModelTestBase, IPerformanceTest
    {
        private static PerformanceTestBase testBase;

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
            OpenModel(testBase.DynamoFilePath);
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

        public IEnumerable<string> PerformanceTestSource()
        {
            return testBase.PerformanceTestSource();
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

            var summary = BenchmarkRunner.Run<DynamoViewModelPerformanceTestBase>(
                new DynamoViewModelPerformanceTestBase.testBase.BenchmarkConfig(args[0]));
        }
    }
}
