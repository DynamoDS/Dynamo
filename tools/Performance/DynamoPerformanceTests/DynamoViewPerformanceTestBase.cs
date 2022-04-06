using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DynamoCoreWpfTests;

namespace DynamoPerformanceTests
{

    public class DynamoViewPerformanceTestBase : GrapViewTests
    {
        /// <summary>
        /// Override this function to preload dlls into Dynamo library
        /// </summary>
        /// <param name="libraries">extra dlls to load</param>
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.AddRange(PerformanceTestHelper.getDynamoDefaultLibs());
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// Test directory containing the graph suite which the current performance test will be running against
        /// </summary>
        public static string testDirectory;

        /// <summary>
        /// Automated creation of performance test cases, one for each
        /// parameter source. Notice this attribute must be public with
        /// [ParamsSource] tag which will throw runtime error otherwise.
        /// </summary>
        [ParamsSource(nameof(PerformanceTestSource))]
        public static string DynamoFilePath { get; set; }

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// Notice this function must be public as well because it is 
        /// defined as the ParamsSource of DynamoFilePath property.
        /// Console app will throw runtime error otherwise.
        /// </summary>
        /// <returns>A list of graph paths for running performance benchmarks</returns>
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
                Console.WriteLine("Running performance benchmarks using graphs at " + fileInfo.FullName);
                yield return fileInfo.FullName;
            }
        }

        #region Iteration setup and cleanup methods for Benchmarks
        /// <summary>
        /// Setup method to be called before each OpenGraph benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(OpenGraph))]
        public void IterationSetupOpenModelWithUI()
        {
            Start();
        }

        /// <summary>
        /// Setup method to be called before each RunGraph benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(RunGraph))]
        public void IterationSetupRunModelWithUI()
        {
            Start();

            // open the dyn file
            Open(DynamoFilePath);

            // Disable tessellation or rendering
            DisableRendering();
        }

        /// <summary>
        /// Setup method to be called before each GraphTessellationAndRendering  benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(GraphTessellationAndRendering))]
        public void IterationSetupRenderGraph()
        {
            Start();

            //open the dyn file
            Open(DynamoFilePath);
            // Disable tessellation or rendering
            DisableRendering();

            Run();
        }

        /// <summary>
        /// Cleanup method to be called after each benchmark.
        /// </summary>
        [IterationCleanup]
        public void IterationCleanup()
        {
            Exit();
        }

        #endregion

        #region Benchmark methods

        // The calling thread must be STA as a requirement
        // Otherwise, System.InvalidOperationException will be thrown during RunIteration
        [Benchmark, System.STAThread]
        public void OpenGraph()
        {
            //open the dyn file
            Open(DynamoFilePath);
        }

        [Benchmark, System.STAThread]
        public void RunGraph()
        {
            Run();
        }

        [Benchmark, System.STAThread]
        public void GraphTessellationAndRendering()
        {
            Tessellate();
        }

        #endregion
    }
}
