using System;
using System.Collections.Generic;
using System.Linq;

namespace GCBenchmark
{
    /// <summary>
    /// Holds the results of a single benchmark test.
    /// </summary>
    public class BenchmarkResult
    {
        public string TestName { get; set; }
        public List<double> DurationsMs { get; } = new List<double>();
        public List<int> GCCounts { get; } = new List<int>();
        public List<long> Allocations { get; } = new List<long>();

        public double MeanMs => DurationsMs.Count > 0 ? DurationsMs.Average() : 0;
        public double MinMs => DurationsMs.Count > 0 ? DurationsMs.Min() : 0;
        public double MaxMs => DurationsMs.Count > 0 ? DurationsMs.Max() : 0;

        public double StdDevMs
        {
            get
            {
                if (DurationsMs.Count < 2) return 0;
                var mean = MeanMs;
                var sumSquares = DurationsMs.Sum(d => (d - mean) * (d - mean));
                return Math.Sqrt(sumSquares / (DurationsMs.Count - 1));
            }
        }

        public double MeanGCCount => GCCounts.Count > 0 ? GCCounts.Average() : 0;
        public double MeanAllocations => Allocations.Count > 0 ? Allocations.Average() : 0;

        public void AddIteration(double durationMs, int gcCount, long allocatedBytes)
        {
            DurationsMs.Add(durationMs);
            GCCounts.Add(gcCount);
            Allocations.Add(allocatedBytes);
        }
    }
}
