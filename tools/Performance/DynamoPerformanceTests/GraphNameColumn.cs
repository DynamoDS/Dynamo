using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;


namespace DynamoPerformanceTests
{
    public class GraphNameColumn : IColumn
    {
        public GraphNameColumn() { }
        
        public string ColumnName => "Graph";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var path = benchmarkCase.Parameters["DynamoFilePath"].ToString();
            var fi = new FileInfo(path);
            return Path.GetFileNameWithoutExtension(fi.Name);
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
        
        public string Id => nameof(GraphNameColumn);
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => true;
        public bool IsAvailable(Summary summary) => true;
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Job;
        public int PriorityInCategory => 0;
        public bool IsNumeric => false;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"Name of test graph";
        public override string ToString() => ColumnName;
    }
}
