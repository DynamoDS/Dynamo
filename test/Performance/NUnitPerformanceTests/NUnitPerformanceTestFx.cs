using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo;

namespace NUnitPerformanceTests
{

    [TestFixture]
    public class NUnitPerformanceTestFx : DynamoModelTestBase
    {
        //private string path;

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

        private static string path
        {
            get
            {
                return Path.Combine(@"C:\Users\pratapa.ADS\Documents\Dynamo\", "perf.txt");
            }
        }

        [Test, TestCaseSource(nameof(PerformanceTestSource)), Category("PerformanceTests")]
        public void Benchmarks(string dynamoFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");

            Stopwatch timer = Stopwatch.StartNew();
            timer.Start();

            //open the dyn file
            OpenModel(dynamoFilePath);

            timer.Stop();

            if (!File.Exists(path))
            {
                File.Create(path);
                using (var tw = new StreamWriter(path))
                {
                    tw.WriteLine("open time for {0} : {1}", dynamoFilePath, timer.ElapsedMilliseconds);
                }
            }
            else
            {
                using (var tw = new StreamWriter(path, true))
                {
                    tw.WriteLine("open time for {0} : {1}", dynamoFilePath, timer.ElapsedMilliseconds);
                }

            }

            //run the expression 
            timer = Stopwatch.StartNew();
            timer.Start();

            BeginRun();

            timer.Stop();
            using (var tw = new StreamWriter(path, true))
            {
                tw.WriteLine("run time for {0} : {1}", dynamoFilePath, timer.ElapsedMilliseconds);
            }
        }

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
}
