using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoPerformanceTests
{
    public class ResultsComparer
    {
        private class BenchmarkResult
        {
            public string Method { get; set; }
            public string Graph { get; set; }
            public double Mean { get; set; }
            public double Error { get; set; }
            public double StdDev { get; set; }
            public double Median { get; set; }
        }

        public ResultsComparer(string basePath, string diffPath)
        {

        }

        private IEnumerable<BenchmarkResult> ImportResultsCSV(string csvPath)
        {

        }
    }
}
