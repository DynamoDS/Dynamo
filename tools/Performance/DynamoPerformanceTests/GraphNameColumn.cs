using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;


namespace DynamoPerformanceTests
{
    /// <summary>
    /// This column displays the file name (without extension) of performance test Dynamo graphs.
    /// </summary>
    public class GraphNameColumn : IColumn
    {
        /// <summary>
        /// The name of this column -- becomes column header in results table.
        /// </summary>
        public string ColumnName => "Graph";

        /// <summary>
        /// Get the file name of the Dynamo graph used in a benchmark.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="benchmarkCase"></param>
        /// <returns>File name of the Dynamo graph used in benchmark</returns>
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var path = benchmarkCase.Parameters["DynamoFilePath"].ToString();
            var fi = new FileInfo(path);
            return Path.GetFileNameWithoutExtension(fi.Name);
        }

        /// <summary>
        /// Get the file name of the Dynamo graph used in a benchmark.
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="benchmarkCase"></param>
        /// <param name="style"></param>
        /// <returns>File name of the Dynamo graph used in benchmark</returns>
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

        /// <summary>
        /// Explanation of column to appear in the results table legend.
        /// </summary>
        public string Legend => $"Name of test graph";

        /// <summary>
        /// Column Category for this column. 
        /// Determines column order in results table.
        /// </summary>
        public ColumnCategory Category => ColumnCategory.Job;

        /// <summary>
        /// Column Category Priority for this column. 
        /// Secondary determination of column order in results table.
        /// </summary>
        public int PriorityInCategory => 0;

        public string Id => nameof(GraphNameColumn);
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => true;
        public bool IsAvailable(Summary summary) => true;
        public bool AlwaysShow => true;
        public bool IsNumeric => false;
        public UnitType UnitType => UnitType.Dimensionless;
        public override string ToString() => ColumnName;
    }
}
