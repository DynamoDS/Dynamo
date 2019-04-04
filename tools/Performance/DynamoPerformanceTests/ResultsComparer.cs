using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// Class that compares two sets of performance results: 
    /// the "base" results, and the "diff" results.
    /// </summary>
    public class ResultsComparer
    {
        private class BenchmarkResult
        {
            /// <summary>
            /// Name of the benchmark method
            /// </summary>
            public string Method { get; set; }

            /// <summary>
            /// Name of test graph
            /// </summary>
            public string Graph { get; set; }

            /// <summary>
            /// Mean benchmark time
            /// </summary>
            public double Mean { get; set; }

            /// <summary>
            /// Error in results
            /// </summary>
            public double Error { get; set; }

            /// <summary>
            /// Standard deviation of results
            /// </summary>
            public double StdDev { get; set; }
        }

        private Dictionary<string, BenchmarkResult> BaseResults;

        private Dictionary<string, BenchmarkResult> DiffResults;

        private Dictionary<string, BenchmarkResult> Comparison;

        /// <summary>
        /// Creates a results comparer to compare results at basePath and diffPath
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="diffPath"></param>
        public ResultsComparer(string basePath, string diffPath)
        {
            BaseResults = ImportResultsCSV(basePath);
            DiffResults = ImportResultsCSV(diffPath);

            Compare();
        }

        /// <summary>
        /// Compare the two results files and build comparison dictionary
        /// </summary>
        private void Compare()
        {
            Comparison = new Dictionary<string, BenchmarkResult>();

            foreach(var result in DiffResults.Values)
            {
                var baseResult = GetBaseResult(result);
                Comparison[result.Method + result.Graph] = new BenchmarkResult()
                {
                    Method = result.Method,
                    Graph = result.Graph,
                    Mean = result.Mean / baseResult.Mean,
                    Error = result.Error / baseResult.Error,
                    StdDev = result.StdDev / baseResult.StdDev,
                };
            }
        }

        public void WriteResultsCSV(string filePath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Method,Graph,Version,Mean,Error,StdDev");
            foreach(var result in DiffResults.Values)
            {
                var baseResult = GetBaseResult(result);
                var baseLine = new string[] { result.Method, result.Graph, "Base", baseResult.Mean.ToString(), baseResult.Error.ToString(), baseResult.StdDev.ToString() };
                csv.AppendLine(string.Join(",", baseLine));

                var diffLine = new string[] { "", "", "Diff", result.Mean.ToString(), result.Error.ToString(), result.StdDev.ToString() };
                csv.AppendLine(string.Join(",", diffLine));

                var compResult = Comparison[result.Method + result.Graph];
                var compLine = new string[] { "", "", "", compResult.Mean.ToString(), compResult.Error.ToString(), compResult.StdDev.ToString() };
                csv.AppendLine(string.Join(",", compLine));
            }

            filePath = GetFullPath(filePath);
            filePath += "/comparison.csv";
            File.WriteAllText(filePath, csv.ToString());
        }

        private BenchmarkResult GetBaseResult(BenchmarkResult result)
        {
            var key = result.Method + result.Graph;
            if (BaseResults.ContainsKey(key))
            {
                return BaseResults[key];
            }
            return null;
        }

        private Dictionary<string, BenchmarkResult> ImportResultsCSV(string csvPath)
        {
            csvPath = GetFullPath(csvPath);
            TextFieldParser parser = new TextFieldParser(csvPath);
            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            var header = parser.ReadFields();
            var iMethod = Array.IndexOf(header, "Method");
            var iGraph = Array.IndexOf(header, "Graph");
            var iMean = Array.IndexOf(header, "Mean");
            var iError = Array.IndexOf(header, "Error");
            var iStdDev = Array.IndexOf(header, "StdDev");

            var benchmarkResults = new Dictionary<string, BenchmarkResult>();
            while (!parser.EndOfData)
            {
                var brLine = parser.ReadFields();
                var method = brLine[iMethod];
                var graph = brLine[iGraph];
                var mean = Convert.ToDouble(Regex.Replace(brLine[iMean], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);
                var error = Convert.ToDouble(Regex.Replace(brLine[iError], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);
                var stdDev = Convert.ToDouble(Regex.Replace(brLine[iStdDev], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);
                benchmarkResults[method+graph] = (
                    new BenchmarkResult()
                    {
                        Method = method,
                        Graph = graph,
                        Mean = mean,
                        Error = error,
                        StdDev = stdDev,
                    }
                );
            }
            return benchmarkResults;
        }
        

        private string GetFullPath(string path)
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string dir = fi.DirectoryName;
            string f = Path.Combine(dir, path);
            return Path.GetFullPath(f);
        }
    }
}
