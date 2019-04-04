using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Dynamo;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// Class utilizing BenchmarkDotNet to implement a performance
    /// benchmarking framework. This class currently measures Open and Run times
    /// for DYN graphs in Dynamo.
    /// </summary>
    public class DynamoModelPerformanceTestBase : DynamoModelTestBase
    {
        /// <summary>
        /// Automated creation of performance test cases, one for each
        /// parameter source.
        /// </summary>
        [ParamsSource(nameof(PerformanceTestSource))]
        private static string DynamoFilePath { get; set; }

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> PerformanceTestSource()
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
                Console.WriteLine("Running performance benchmarks using graphs at " + fileInfo.FullName);
                yield return fileInfo.FullName;
            }
        }

        /// <summary>
        /// Override this function to preload dlls into Dynamo library
        /// </summary>
        /// <param name="libraries">extra dlls to load</param>
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
        /// Test directory containing the graph suite which the current performance test will be running against
        /// </summary>
        public static string testDirectory;

        #region Iteration setup and cleanup methods for Benchmarks

        /// <summary>
        /// Setup method to be called before each OpenModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Open))]
        public void IterationSetupOpenModel()
        {
            Setup();
        }

        /// <summary>
        /// Setup method to be called before each RunModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Run))]
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
        public void Open()
        {
            //open the dyn file
            OpenModel(DynamoFilePath);
        }

        [Benchmark]
        public void Run()
        {
            BeginRun();
        }

        #endregion
    }
}
