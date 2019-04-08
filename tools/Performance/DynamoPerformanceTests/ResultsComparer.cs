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
            internal string Method { get; set; }

            /// <summary>
            /// Name of test graph
            /// </summary>
            internal string Graph { get; set; }

            /// <summary>
            /// Mean benchmark time
            /// </summary>
            internal double Mean { get; set; }

            /// <summary>
            /// Error in results
            /// </summary>
            internal double Error { get; set; }

            /// <summary>
            /// Standard deviation of results
            /// </summary>
            internal double StdDev { get; set; }
        }

        private class BenchmarkComparison
        {
            /// <summary>
            /// Results of base benchmark
            /// </summary>
            internal BenchmarkResult Base;

            /// <summary>
            /// Results of diff benchmark
            /// </summary>
            internal BenchmarkResult Diff;

            /// <summary>
            /// Diff mean time as a percentage of base mean time
            /// </summary>
            internal double MeanDelta
            {
                get { return Math.Round(100 * Diff.Mean / Base.Mean, 2); }
            }

            /// <summary>
            /// Diff error as a percentage of base error
            /// </summary>
            internal double ErrorDelta
            {
                get { return Math.Round(100 * Diff.Error / Base.Error, 2); }
            }

            /// <summary>
            /// Diff standard deviation as a percentage of base standard deviation
            /// </summary>
            internal double StdDevDelta
            {
                get { return Math.Round(100 * Diff.StdDev / Base.StdDev, 2); }
            }

            /// <summary>
            /// Indicates whether a baseline benchmark result was found
            /// that matches this diff benchmark
            /// </summary>
            internal bool BaseBenchmarkFound
            {
                get { return Base != null; }
            }

            /// <summary>
            /// Create a Comparison between two benchamrk results
            /// </summary>
            /// <param name="baseData"></param>
            /// <param name="diffData"></param>
            internal BenchmarkComparison(BenchmarkResult baseData, BenchmarkResult diffData)
            {
                Base = baseData;
                Diff = diffData;

                // Check that these results refer to the same graph and benchmark
                if (Base.Method != Diff.Method || Base.Graph != Diff.Graph)
                {
                    throw new Exception("Non-matching benchmarks provided for comparison");
                }
            }

            /// <summary>
            /// Log the results of this comparison to the console
            /// </summary>
            /// <param name="columnWidths"></param>
            internal void LogComparison(List<int> columnWidths)
            {
                // Make sure baseline data has been found. If not, log an error message for this comparison.
                if (!BaseBenchmarkFound)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Baseline bechmark results do not contain data for " + Diff.Method + "-" + Diff.Graph);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }

                // Log Base data
                var baseData = new string[] { Base.Method, Base.Graph, "Base", Base.Mean.ToString(), Base.Error.ToString(), Base.StdDev.ToString() };
                for (int i = 0; i < baseData.Length; i++)
                {
                    baseData[i] = (baseData[i] + " ").PadLeft(columnWidths[i], ' ');
                }
                Console.WriteLine("|" + string.Join("|", baseData) + "|");

                // Log Diff data
                var diffData = new string[] { "", "", "Diff", Diff.Mean.ToString(), Diff.Error.ToString(), Diff.StdDev.ToString() };
                for (int i = 0; i < diffData.Length; i++)
                {
                    diffData[i] = (diffData[i] + " ").PadLeft(columnWidths[i], ' ');
                }
                Console.WriteLine("|" + string.Join("|", diffData) + "|");

                // Lof Delta data
                var deltaData = new string[] { "", "", MeanDelta <= 100 ? "+" : "-", MeanDelta.ToString() + "%", ErrorDelta.ToString() + "%", StdDevDelta.ToString() + "%" };
                Console.Write("|");
                for (int i = 0; i < deltaData.Length; i++)
                {
                    var deltaItem = (deltaData[i] + " ").PadLeft(columnWidths[i], ' ');
                    if (i == 3)
                    {
                        Console.ForegroundColor = MeanDelta <= 100 ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.Write(deltaItem);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("|");
                    }
                    else
                    {
                        Console.Write(deltaItem + "|");
                    }
                }
                Console.Write("\n");
            }

            /// <summary>
            /// Get the results of this comparison as a list of arrays of strings
            /// </summary>
            /// <returns></returns>
            internal List<string[]> GetComparisonData()
            {
                var baseData = new string[] { Base.Method, Base.Graph, "Base", Base.Mean.ToString(), Base.Error.ToString(), Base.StdDev.ToString() };
                var diffData = new string[] { "", "", "Diff", Diff.Mean.ToString(), Diff.Error.ToString(), Diff.StdDev.ToString() };
                var deltaData= new string[] { "", "", MeanDelta <= 100 ? "+" : "-", MeanDelta.ToString() + "%", ErrorDelta.ToString() + "%", StdDevDelta.ToString() + "%" };
                return new List<string[]> { baseData, diffData, deltaData };
            }

        }
        
        private List<BenchmarkComparison> Comparisons;

        private string BasePath;
        private string DiffPath;

        /// <summary>
        /// Creates a results comparer to compare results at basePath and diffPath
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="diffPath"></param>
        public ResultsComparer(string basePath, string diffPath)
        {
            BasePath = basePath;
            DiffPath = diffPath;

            CreateComparisons();
            LogResults();
        }

        /// <summary>
        /// Compare the two results files and build comparison data
        /// </summary>
        private void CreateComparisons()
        {
            var baseResults = ImportResultsCSV(BasePath);
            var diffResults = ImportResultsCSV(DiffPath);

            Comparisons = new List<BenchmarkComparison>();

            foreach(var result in diffResults.Values)
            {
                var baseResult = GetBaseResult(result, baseResults);
                Comparisons.Add(new BenchmarkComparison(baseResult, result));
            }
        }

        /// <summary>
        /// Print comparison results to the console
        /// </summary>
        private void LogResults()
        {
            var columnWidths = GetColumnWidths();

            // Log header
            var header = new string[] { "Method", "Graph", "Version", "Mean", "Error", "StdDev" };
            var separator = new string[header.Length];
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = (header[i] + " ").PadLeft(columnWidths[i], ' ');
                separator[i] = "".PadLeft(columnWidths[i], '-');
            }
            Console.WriteLine("|" + string.Join("|", header) + "|");
            Console.WriteLine("|" + string.Join("|", separator) + "|");
            
            // Log comparisons
            foreach (var comparison in Comparisons)
                comparison.LogComparison(columnWidths);
        }

        /// <summary>
        /// Write the results of this comparison to a csv file
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteResultsToCSV(string filePath)
        {
            var rows = GetComparisonDataRows().Select(r => string.Join(",", r));
            var csv = new StringBuilder();
            foreach (var row in rows) csv.AppendLine(row);

            filePath = GetFullPath(filePath);
            filePath += "/comparison.csv";
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private Dictionary<string, BenchmarkResult> ImportResultsCSV(string csvPath)
        {
            // Get csv
            csvPath = GetFullPath(csvPath);
            TextFieldParser parser = new TextFieldParser(csvPath);
            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            // Get csv header
            var header = parser.ReadFields();

            // Check that columns we care about exist
            var columnNames = new string[] { "Method", "Graph", "Mean", "Error", "StdDev" };
            CheckCSVHeader(header, columnNames, csvPath);

            // Get indices for columns we care about
            var iMethod = Array.IndexOf(header, columnNames[0]);
            var iGraph = Array.IndexOf(header, columnNames[1]);
            var iMean = Array.IndexOf(header, columnNames[2]);
            var iError = Array.IndexOf(header, columnNames[3]);
            var iStdDev = Array.IndexOf(header, columnNames[4]);

            // Parse results
            var benchmarkResults = new Dictionary<string, BenchmarkResult>();
            while (!parser.EndOfData)
            {
                // Get full benchmark data from csv
                var brLine = parser.ReadFields();

                // Get the values we care about
                var method = brLine[iMethod];
                var graph = brLine[iGraph];
                var mean = Convert.ToDouble(Regex.Replace(brLine[iMean], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);
                var error = Convert.ToDouble(Regex.Replace(brLine[iError], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);
                var stdDev = Convert.ToDouble(Regex.Replace(brLine[iStdDev], "[^0-9|.|,]", ""), CultureInfo.InvariantCulture);

                // Store the benchmark based on its 'Method' and 'Graph' values
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

        private List<string[]> GetComparisonDataRows()
        {
            var rows = new List<string[]>();
            var header = new string[] { "Method", "Graph", "Version", "Mean", "Error", "StdDev" };
            rows.Add(header);

            foreach (var comparison in Comparisons)
            {
                rows.AddRange(comparison.GetComparisonData());
            }
            return rows;
        }

        private List<string>[] GetComparisonDataColumns()
        {
            var rows = GetComparisonDataRows();
            var numColumns = rows[0].Length;

            var columns = new List<string>[numColumns];
            for (int i = 0; i < numColumns; i++)
                columns[i] = new List<string>();

            foreach (var row in rows)
                for (int i = 0; i < numColumns; i++)
                    columns[i].Add(row[i]);

            return columns;
        }

        private List<int> GetColumnWidths()
        {
            var columns = GetComparisonDataColumns();
            var columnWidths = columns.Select(c => c.OrderByDescending(s => s.Length).First().Length + 2).ToList();
            return columnWidths;
        }

        private BenchmarkResult GetBaseResult(BenchmarkResult result, Dictionary<string, BenchmarkResult> baseResults)
        {
            var key = result.Method + result.Graph;
            if (baseResults.ContainsKey(key))
            {
                return baseResults[key];
            }
            return null;
        }

        private void CheckCSVHeader(string[] header, string[] columnNames, string csvPath)
        {
            var columnIndices = columnNames.Select(c => Array.IndexOf(header, c)).ToList();
            if (columnIndices.Where(i => i == -1).Count() > 0)
            {
                var missingColumns = new List<string>();
                for (int i = 0; i < columnNames.Count(); i++)
                {
                    if (columnIndices[i] == -1)
                    {
                        missingColumns.Add(columnNames[i]);
                    }
                }
                var message = string.Format("The csv file at {0} does not contain the following required columns: {1}.", csvPath, string.Join(", ", missingColumns));
                throw new Exception(message);
            }
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
