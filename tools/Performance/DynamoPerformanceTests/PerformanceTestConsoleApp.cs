using System;
using System.IO;
using BenchmarkDotNet.Running;

namespace DynamoPerformanceTests
{
    public class Program
    {
        /// <summary>
        /// This is the entrance of Dynamo Performance Console App
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir;
            // Running with an input dir location:
            // DynamoPerformanceTests.exe "C:\directory path\"
            // 
            if (args.Length <= 0)
            {
                Console.WriteLine("Supply a path to a test directory containing DYN files");
            }
            else
            {
                try
                {
                    dir = new DirectoryInfo(args[0]);
                    Console.WriteLine("Running performance benchmarks using graphs at " + dir.FullName);
                    // Use helper to get debug config in order to run benchmarks on debug build of DynamoCore
                    // PerformanceTestHelper.getDebugConfig();
                    DynamoViewModelPerformanceTestBase.testDirectory = dir.FullName;
                    var runSummaryWithUI = BenchmarkRunner.Run<DynamoViewModelPerformanceTestBase>(PerformanceTestHelper.getFastReleaseConfig());

                    DynamoModelPerformanceTestBase.testDirectory = dir.FullName;
                    var runSummaryWithoutUI = BenchmarkRunner.Run<DynamoModelPerformanceTestBase>(PerformanceTestHelper.getFastReleaseConfig());
                }
                catch
                {
                    Console.WriteLine("Not a valid path");
                }
            }
        }
    }
}
