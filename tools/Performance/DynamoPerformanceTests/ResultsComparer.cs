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
    /// the baseline results, and a set of new results.
    /// </summary>
    public class ResultsComparer
    {
        private class BenchmarkResult
        {
            // Values
            internal string Method { get; set; }
            internal string Graph { get; set; }
            internal double Mean { get; set; }
            internal double Error { get; set; }
            internal double StdDev { get; set; }

            // Units
            internal string MeanUnits { get; set; }
            internal string ErrorUnits { get; set; }
            internal string StdDevUnits { get; set; }

            // Values + Units
            internal string MeanString { get { return Mean.ToString("N", CultureInfo.InvariantCulture) + " " + MeanUnits; } }
            internal string ErrorString { get { return Error.ToString("N", CultureInfo.InvariantCulture) + " " + ErrorUnits; } }
            internal string StdDevString { get { return StdDev.ToString("N", CultureInfo.InvariantCulture) + " " + StdDevUnits; } }

            /// <summary>
            /// Create a Benchmark result from values parsed from results csv
            /// </summary>
            /// <param name="method"></param>
            /// <param name="graph"></param>
            /// <param name="mean"></param>
            /// <param name="meanUnits"></param>
            /// <param name="error"></param>
            /// <param name="errorUnits"></param>
            /// <param name="stdDev"></param>
            /// <param name="stdDevUnits"></param>
            internal BenchmarkResult(string method, string graph, double mean, string meanUnits, double error, string errorUnits, double stdDev, string stdDevUnits)
            {
                Method = method;
                Graph = graph;
                Mean = mean;
                MeanUnits = meanUnits;
                Error = error;
                ErrorUnits = errorUnits;
                StdDev = stdDev;
                StdDevUnits = stdDevUnits;
            }
        }

        private class BenchmarkComparison
        {
            /// <summary>
            /// Results of baseline benchmark
            /// </summary>
            internal BenchmarkResult BaseResult;

            /// <summary>
            /// Results of new benchmark
            /// </summary>
            internal BenchmarkResult NewResult;

            /// <summary>
            /// Change in mean time from base to new
            /// </summary>
            internal double MeanDelta
            {
                get { return Math.Round(100 * (NewResult.Mean - BaseResult.Mean) / BaseResult.Mean, 2); }
            }

            /// <summary>
            /// Change in error from base to new
            /// </summary>
            internal double ErrorDelta
            {
                get { return Math.Round(100 * (NewResult.Error - BaseResult.Error) / BaseResult.Error, 2); }
            }

            /// <summary>
            /// Change in standard deviation from base to new
            /// </summary>
            internal double StdDevDelta
            {
                get { return Math.Round(100 * (NewResult.StdDev - BaseResult.StdDev) / BaseResult.StdDev, 2); }
            }

            /// <summary>
            /// Indicates whether a baseline benchmark result was found
            /// that matches this new benchmark result
            /// </summary>
            internal bool BaseBenchmarkFound
            {
                get { return BaseResult != null; }
            }

            /// <summary>
            /// Create a Comparison between two benchamrk results
            /// </summary>
            /// <param name="baseData"></param>
            /// <param name="newData"></param>
            internal BenchmarkComparison(BenchmarkResult baseData, BenchmarkResult newData)
            {
                BaseResult = baseData;
                NewResult = newData;

                // Check that these results refer to the same graph and benchmark
                if (BaseResult.Method != NewResult.Method || BaseResult.Graph != NewResult.Graph)
                {
                    throw new Exception("Non-matching benchmarks provided for comparison");
                }
                
                if (BaseResult.MeanUnits != NewResult.MeanUnits || BaseResult.ErrorUnits != NewResult.ErrorUnits || BaseResult.StdDevUnits != NewResult.StdDevUnits)
                {
                    var fc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Units do not match beween base and new results.");
                    Console.ForegroundColor = fc;
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
                    var fc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Baseline bechmark results do not contain data for " + NewResult.Method + "-" + NewResult.Graph);
                    Console.ForegroundColor = fc;
                    return;
                }

                // Log Base data
                var baseData = new string[] { BaseResult.Method, BaseResult.Graph, "Base", BaseResult.MeanString, BaseResult.ErrorString, BaseResult.StdDevString };
                for (int i = 0; i < baseData.Length; i++)
                {
                    baseData[i] = (baseData[i] + " ").PadLeft(columnWidths[i], ' ');
                }
                Console.WriteLine("|" + string.Join("|", baseData) + "|");

                // Log New data
                var newData = new string[] { "", "", "New", NewResult.MeanString, NewResult.ErrorString, NewResult.StdDevString };
                for (int i = 0; i < newData.Length; i++)
                {
                    newData[i] = (newData[i] + " ").PadLeft(columnWidths[i], ' ');
                }
                Console.WriteLine("|" + string.Join("|", newData) + "|");

                // Lof Delta data
                var meanDeltaString = MeanDelta >= 0 ? "+" + MeanDelta.ToString() : MeanDelta.ToString();
                var deltaData = new string[] { "", "", "", meanDeltaString + "%", ErrorDelta.ToString() + "%", StdDevDelta.ToString() + "%" };
                Console.Write("|");
                for (int i = 0; i < deltaData.Length; i++)
                {
                    var deltaItem = (deltaData[i] + " ").PadLeft(columnWidths[i], ' ');
                    if (i == 3)
                    {
                        var fc = Console.ForegroundColor;
                        Console.ForegroundColor = MeanDelta < 0 ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.Write(deltaItem);
                        Console.ForegroundColor = fc;
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
                if (!BaseBenchmarkFound) return null;

                var baseData = new string[] { BaseResult.Method, BaseResult.Graph, "Base", BaseResult.MeanString, BaseResult.ErrorString, BaseResult.StdDevString };
                var newData = new string[] { "", "", "New", NewResult.MeanString, NewResult.ErrorString, NewResult.StdDevString };
                var deltaData = new string[] { "", "", "", MeanDelta.ToString() + "%", ErrorDelta.ToString() + "%", StdDevDelta.ToString() + "%" };
                return new List<string[]> { baseData, newData, deltaData };
            }
        }
        
        /// <summary>
        /// Creates a results comparer to compare results at basePath and newPath
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="newPath"></param>
        public ResultsComparer(string basePath, string newPath, string saveCSVPath = null)
        {
            // Create BenchmarkComparison objects
            var comparisons = CreateComparisons(basePath, newPath);
            
            // Log comparisons to console
            LogResults(comparisons);

            // Save comparisons to csv if path provided
            if (!string.IsNullOrEmpty(saveCSVPath))
            {
                WriteResultsToCSV(comparisons, saveCSVPath);
            }
        }

        /// <summary>
        /// Compare the two results files and build comparison data
        /// </summary>
        private List<BenchmarkComparison> CreateComparisons(string basePath, string newPath)
        {
            var baseResults = ImportResultsCSV(basePath);
            var newResults = ImportResultsCSV(newPath);

            var comparisons = new List<BenchmarkComparison>();

            foreach(var result in newResults.Values)
            {
                var baseResult = GetBaseResult(result, baseResults);
                comparisons.Add(new BenchmarkComparison(baseResult, result));
            }
            return comparisons;
        }

        /// <summary>
        /// Print comparison results to the console
        /// </summary>
        private void LogResults(List<BenchmarkComparison> comparisons)
        {
            var columnWidths = GetColumnWidths(comparisons);

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
            foreach (var comparison in comparisons)
            {
                Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.Gray ? ConsoleColor.White : ConsoleColor.Gray;
                comparison.LogComparison(columnWidths);
            }
        }

        /// <summary>
        /// Write the results of this comparison to a csv file
        /// </summary>
        /// <param name="filePath"></param>
        private void WriteResultsToCSV(List<BenchmarkComparison> comparisons, string filePath)
        {
            var rowData = GetComparisonDataRows(comparisons).Select(r => r.Select(x => "\"" + x + "\""));
            var rows = rowData.Select(r => string.Join(",", r));
            var csv = new StringBuilder();
            foreach (var row in rows) csv.AppendLine(row);
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private Dictionary<string, BenchmarkResult> ImportResultsCSV(string csvPath)
        {
            // Get csv
            csvPath = PerformanceTestHelper.GetFullPath(csvPath);
            TextFieldParser parser = new TextFieldParser(csvPath);
            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            // Get csv header
            var header = parser.ReadFields();

            // Check that columns we care about exist
            var columnNames = new string[] { "Method", "Graph", "Mean", "Error", "StdDev" };
            var missingColumns = columnNames.Where(c => !header.Contains(c));
            if (missingColumns.Any())
            {
                throw new Exception(string.Format("The csv file at {0} does not contain the following required columns: {1}.", csvPath, string.Join(", ", missingColumns)));
            }

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
                var meanUnits = new string(brLine[iMean].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
                var errorUnits = new string(brLine[iError].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
                var stdDevUnits = new string(brLine[iStdDev].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());

                // Store the benchmark based on its 'Method' and 'Graph' values
                benchmarkResults[method+graph] = (new BenchmarkResult(method, graph, mean, meanUnits, error, errorUnits, stdDev, stdDevUnits));
            }
            return benchmarkResults;
        }

        private List<string[]> GetComparisonDataRows(List<BenchmarkComparison> comparisons)
        {
            var rows = new List<string[]>();
            var header = new string[] { "Method", "Graph", "Version", "Mean", "Error", "StdDev" };
            rows.Add(header);

            foreach (var comparison in comparisons)
            {
                rows.AddRange(comparison.GetComparisonData());
            }
            return rows;
        }

        private List<int> GetColumnWidths(List<BenchmarkComparison> comparisons)
        {
            var rows = GetComparisonDataRows(comparisons);
            var columnWidths = new int[rows[0].Length];
            for(int i = 0; i < rows.Count; i++)
            {
                for(int j = 0; j < rows[i].Length; j++)
                {
                    if (rows[i][j].Length > columnWidths[j]) columnWidths[j] = rows[i][j].Length;
                }
            }
            columnWidths = columnWidths.Select(c => c + 2).ToArray();
            return columnWidths.ToList();
        }

        private BenchmarkResult GetBaseResult(BenchmarkResult newResult, Dictionary<string, BenchmarkResult> baseResults)
        {
            var key = newResult.Method + newResult.Graph;
            if (baseResults.ContainsKey(key))
            {
                return baseResults[key];
            }
            return null;
        }
    }
}
