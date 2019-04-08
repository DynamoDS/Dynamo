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
        private int numColumns = 6;

        private string[] ColumnNames = new string[] { "Method", "Graph", "Version", "Mean", "Error", "StdDev" };

        private class ComparisonData
        {
            public Dictionary<string, object> Base;

            public Dictionary<string, object> Diff;
        }

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
            LogResults();
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
                    Mean = Math.Round(100 * result.Mean / baseResult.Mean, 2),
                    Error = Math.Round(100 * result.Error / baseResult.Error, 2),
                    StdDev = Math.Round(100 * result.StdDev / baseResult.StdDev, 2),
                };
            }
        }

        private List<string[]> GetComparisonDataRows()
        {
            var rows = new List<string[]>();
            rows.Add(ColumnNames);

            foreach (var result in DiffResults.Values)
            {
                var baseResult = GetBaseResult(result);
                var baseLine = new string[] { result.Method, result.Graph, "Base", baseResult.Mean.ToString(), baseResult.Error.ToString(), baseResult.StdDev.ToString() };
                rows.Add(baseLine);

                var diffLine = new string[] { "", "", "Diff", result.Mean.ToString(), result.Error.ToString(), result.StdDev.ToString() };
                rows.Add(diffLine);

                var compResult = Comparison[result.Method + result.Graph];
                var compLine = new string[] { "", "", compResult.Mean < 100 ? "+" : "-", compResult.Mean.ToString() + "%", compResult.Error.ToString() + "%", compResult.StdDev.ToString() + "%" };
                rows.Add(compLine);
            }
            return rows;
        }

        private List<string>[] GetComparisonDataColumns()
        {
            var columns = new List<string>[numColumns];
            for (int i = 0; i < numColumns; i++)
            {
                columns[i] = new List<string>();
            }

            var rows = GetComparisonDataRows();
            foreach(var row in rows)
            {
                for (int i = 0; i < numColumns; i++)
                {
                    columns[i].Add(row[i]);
                }
            }
            return columns;
        }

        private void LogResults()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            var columns = GetComparisonDataColumns();
            var columnWidths = columns.Select(c => c.OrderByDescending(s => s.Length).First().Length + 1).ToList();
            for (int i = 0; i < columns[0].Count; i++)
            {
                if ( i % 3 == 1)
                {
                    //Console.WriteLine("".PadLeft(columnWidths.Sum()+ 13, '-'));
                }
                Console.Write("|");
                for (int j = 0; j < numColumns; j++)
                {
                    var dataItem = columns[j][i].PadLeft(columnWidths[j], ' ');
                    dataItem += " |";
                    Console.Write(dataItem);
                }
                Console.Write("\n");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }



        public void WriteResultsCSV(string filePath)
        {
            var rows = GetComparisonDataRows().Select(r => string.Join(",", r));
            var csv = new StringBuilder();
            foreach (var row in rows) csv.AppendLine(row);
            
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
            var columnIndices = ColumnNames.Select(c => Array.IndexOf(header, c)).ToList();
            if (columnIndices.Where(i => i == -1).Count() > 0)
            {
                var missingColumns = new List<string>();
                for(int i = 0; i < ColumnNames.Count(); i++)
                {
                    if (columnIndices[i] == -1)
                    {
                        missingColumns.Add(ColumnNames[i]);
                    }
                }
                var message = string.Format("The csv file at {0} does not contain the following required columns: {1}.", csvPath, string.Join(", ", missingColumns));
                throw new Exception(message);
            }

            var iMethod = Array.IndexOf(header, "Method");
            var iGraph = Array.IndexOf(header, "Graph");
            var iMean = Array.IndexOf(header, "Mean");
            var iError = Array.IndexOf(header, "Error");
            var iStdDev = Array.IndexOf(header, "StdDev");

            var benchmarkResults = new Dictionary<string, BenchmarkResult>();

            var data = new Dictionary<string, IEnumerable<string>>();
            while (!parser.EndOfData)
            {
                // Get full benchmark data from csv
                var brLine = parser.ReadFields();

                // Get only the values that we care about
                var valuesToKeep = new List<string>();
                foreach (var i in columnIndices) valuesToKeep.Add(brLine[i]);

                // Store the benchmark based on its Method and Graph values
                data[valuesToKeep[0] + valuesToKeep[1]] = valuesToKeep;

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
