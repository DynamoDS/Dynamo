using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;

namespace GCBenchmark
{
    /// <summary>
    /// Command line options for the GC Benchmark tool.
    /// </summary>
    public class Options
    {
        [Option('i', "iterations", Default = 10, HelpText = "Number of iterations per test")]
        public int Iterations { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output file path for results (markdown format)")]
        public string OutputPath { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; set; }

        [Option('t', "tests", Default = null, Separator = ',', HelpText = "Specific tests to run (comma-separated)")]
        public IEnumerable<string> Tests { get; set; }

        [Option('w', "warmup", Default = 2, HelpText = "Number of warmup iterations")]
        public int WarmupIterations { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    options => RunBenchmarks(options),
                    errors => 1);
        }

        static int RunBenchmarks(Options options)
        {
            Console.WriteLine("=== DesignScript GC Benchmark Tool ===");
            Console.WriteLine($"Iterations: {options.Iterations}");
            Console.WriteLine($"Warmup: {options.WarmupIterations}");
            Console.WriteLine();

            var runner = new GCBenchmarkRunner(options);
            var results = runner.RunAllBenchmarks();

            // Print results to console
            PrintResults(results, Console.Out);

            // Write results to file if specified
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                using (var writer = new StreamWriter(options.OutputPath))
                {
                    WriteMarkdownResults(results, writer, options);
                }
                Console.WriteLine($"\nResults written to: {options.OutputPath}");
            }

            return 0;
        }

        static void PrintResults(List<BenchmarkResult> results, TextWriter writer)
        {
            writer.WriteLine("\n=== Results ===\n");
            writer.WriteLine($"{"Test",-45} {"Mean (ms)",12} {"StdDev",10} {"Min",10} {"Max",10} {"GC Count",10}");
            writer.WriteLine(new string('-', 100));

            foreach (var result in results)
            {
                writer.WriteLine($"{result.TestName,-45} {result.MeanMs,12:F3} {result.StdDevMs,10:F3} {result.MinMs,10:F3} {result.MaxMs,10:F3} {result.MeanGCCount,10:F1}");
            }
        }

        static void WriteMarkdownResults(List<BenchmarkResult> results, TextWriter writer, Options options)
        {
            writer.WriteLine("# GC Benchmark Results");
            writer.WriteLine();
            writer.WriteLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"**Iterations:** {options.Iterations}");
            writer.WriteLine($"**Warmup:** {options.WarmupIterations}");
            writer.WriteLine();
            writer.WriteLine("## Summary");
            writer.WriteLine();
            writer.WriteLine("| Test | Mean (ms) | StdDev | Min | Max | GC Count |");
            writer.WriteLine("|------|-----------|--------|-----|-----|----------|");

            foreach (var result in results)
            {
                writer.WriteLine($"| {result.TestName} | {result.MeanMs:F3} | {result.StdDevMs:F3} | {result.MinMs:F3} | {result.MaxMs:F3} | {result.MeanGCCount:F1} |");
            }

            writer.WriteLine();
            writer.WriteLine("## Detailed Results");
            writer.WriteLine();

            foreach (var result in results)
            {
                writer.WriteLine($"### {result.TestName}");
                writer.WriteLine();
                writer.WriteLine($"- **Mean:** {result.MeanMs:F3} ms");
                writer.WriteLine($"- **Std Dev:** {result.StdDevMs:F3} ms");
                writer.WriteLine($"- **Min:** {result.MinMs:F3} ms");
                writer.WriteLine($"- **Max:** {result.MaxMs:F3} ms");
                writer.WriteLine($"- **GC Count (mean):** {result.MeanGCCount:F1}");
                writer.WriteLine($"- **Allocations (mean):** {result.MeanAllocations:N0} bytes");
                writer.WriteLine();
            }
        }
    }
}
