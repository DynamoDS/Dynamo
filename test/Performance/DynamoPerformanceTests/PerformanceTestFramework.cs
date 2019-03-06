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
    public class PerformanceTestFramework : DynamoModelTestBase
    {
        public class AllowNonOptimized : ManualConfig
        {
            public AllowNonOptimized()
            {
                Add(JitOptimizationsValidator.DontFailOnError);

                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
            }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            base.GetLibrariesToPreload(libraries);

            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSOffice.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
        }

        [ParamsSource(nameof(PerformanceTestSource))]
        public string DynamoFilePath { get; set; }

        #region Iteration setup and cleanup methods for Benchmarks

        [IterationSetup(Target = nameof(OpenModelBenchmark))]
        public void IterationSetupOpenModel()
        {
            Setup();
        }

        [IterationSetup(Target = nameof(RunModelBenchmark))]
        public void IterationSetupRunModel()
        {
            Setup();

            //open the dyn file
            OpenModel(DynamoFilePath);
        }


        [IterationCleanup]
        public void IterationCleanup()
        {
            Cleanup();
        }

        #endregion

        #region Benchmark methods
        /// <summary>
        /// Automated creation of performance test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
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
            // TODO: to be parameterized
            string testsLoc = Path.Combine(dir, @"..\..\..\test\core\WorkflowTestFiles\ListManagementMisc");
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
            //var summary = BenchmarkRunner.Run<PerformanceTestFramework>(new PerformanceTestFramework.AllowNonOptimized());
            var summary = BenchmarkRunner.Run<PerformanceTestFramework>();
        }
    }
}
