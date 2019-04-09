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
            internal string MeanString { get { return Mean.ToString() + " " + MeanUnits; } }
            internal string ErrorString { get { return Error.ToString() + " " + ErrorUnits; } }
            internal string StdDevString { get { return StdDev.ToString() + " " + StdDevUnits; } }
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
        
        private List<BenchmarkComparison> Comparisons;

        private string BasePath;
        private string NewPath;

        /// <summary>
        /// Creates a results comparer to compare results at basePath and newPath
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="newPath"></param>
        public ResultsComparer(string basePath, string newPath)
        {
            BasePath = basePath;
            NewPath = newPath;

            CreateComparisons();
            LogResults();
        }

        /// <summary>
        /// Compare the two results files and build comparison data
        /// </summary>
        private void CreateComparisons()
        {
            var baseResults = ImportResultsCSV(BasePath);
            var newResults = ImportResultsCSV(NewPath);

            Comparisons = new List<BenchmarkComparison>();

            foreach(var result in newResults.Values)
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
            {
                Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.Gray ? ConsoleColor.White : ConsoleColor.Gray;
                comparison.LogComparison(columnWidths);

            }
                
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
                var meanUnits = new string(brLine[iMean].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
                var errorUnits = new string(brLine[iMean].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
                var stdDevUnits = new string(brLine[iMean].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());

                // Store the benchmark based on its 'Method' and 'Graph' values
                benchmarkResults[method+graph] = (
                    new BenchmarkResult()
                    {
                        Method = method,
                        Graph = graph,
                        Mean = mean,
                        Error = error,
                        StdDev = stdDev,
                        MeanUnits = meanUnits,
                        ErrorUnits = errorUnits,
                        StdDevUnits = stdDevUnits,
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

        private BenchmarkResult GetBaseResult(BenchmarkResult newResult, Dictionary<string, BenchmarkResult> baseResults)
        {
            var key = newResult.Method + newResult.Graph;
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
