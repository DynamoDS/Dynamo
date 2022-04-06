using System.Collections.Generic;
using System.IO;
using System.Linq;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    /// <summary>
    /// Runs the performance graphs as visualization tests. Their purpose is to provide rendering timings only.
    /// For that, you should make sure the renderTimer Stopwatch in HelixWatch3DViewModel is enabled and logging to Console.
    /// You should run test including only the PerformanceVisualization category. Output should be redirected to a file.
    /// You can extract the timings using grep and output them to a CSV file for ease of postprocessing.
    /// </summary>
    [TestFixture]
    [Category("PerformanceVisualization")]
    [Category("Failure")] // Not really a failure, we just don't want them to run on a normal build.
    public class PerformanceVisualizationTests : VisualizationTest
    {
        // Determines how many times each performance graph will be run.
        const int SamplingAmount = 3;

        [Test]
        [TestCaseSource("PerformanceTestSource")]
        public void TestVisualizationPerformance(string filePath)
        {
            ViewModel.OpenCommand.Execute(filePath);
            DispatcherUtil.DoEvents();
            RunCurrentModel();
            DispatcherUtil.DoEvents();
        }

        public IEnumerable<string> PerformanceTestSource
        {
            get
            {
                var path = Path.Combine(GetTestDirectory(ExecutingDirectory), @"..\tools\Performance\DynamoPerformanceTests\graphs");
                return new DirectoryInfo(path).GetFiles("*.dyn").SelectMany(fileInfo => Enumerable.Repeat(fileInfo.FullName, SamplingAmount));
            }
        }
    }
}
