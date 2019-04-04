using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Dynamo.Tests;

namespace DynamoPerformanceTests
{

    public class DynamoViewModelPerformanceTestBase : DynamoViewModelUnitTest
    {
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

        /// <summary>
        /// Automated creation of performance test cases, one for each
        /// parameter source. Notice this attribute must be public with
        /// [ParamsSource] tag which will only thrown runtime error otherwise.
        /// </summary>
        [ParamsSource(nameof(PerformanceTestSource))]
        public static string DynamoFilePath { get; set; }

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// Notice this function must be public as well because it is 
        /// defined as the ParamsSource of DynamoFilePath property.
        /// Console app will throw runtime error otherwise.
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
                Console.WriteLine("Running performance benchmarks using graphs at " + fileInfo.FullName);
                yield return fileInfo.FullName;
            }
        }

        #region Iteration setup and cleanup methods for Benchmarks
        /// <summary>
        /// Setup method to be called before each OpenModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Open))]
        public void IterationSetupOpenModelWithUI()
        {
            Setup();
        }

        /// <summary>
        /// Setup method to be called before each RunModel benchmark.
        /// </summary>
        [IterationSetup(Target = nameof(Run))]
        public void IterationSetupRunModelWithUI()
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
            RunCurrentModel();
        }

        #endregion
    }
}
