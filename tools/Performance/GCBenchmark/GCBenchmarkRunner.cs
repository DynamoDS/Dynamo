using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;

using CoreOptions = ProtoCore.Options;
using BenchmarkOptions = GCBenchmark.Options;

namespace GCBenchmark
{
    /// <summary>
    /// Runs GC-focused benchmarks against the DesignScript engine.
    /// </summary>
    public class GCBenchmarkRunner
    {
        private readonly BenchmarkOptions _options;
        private readonly Dictionary<string, Func<BenchmarkResult>> _tests;

        public GCBenchmarkRunner(BenchmarkOptions options)
        {
            _options = options;
            _tests = new Dictionary<string, Func<BenchmarkResult>>
            {
                ["SimpleAllocation"] = () => RunTest("SimpleAllocation", SimpleAllocationTest),
                ["ArrayAllocation"] = () => RunTest("ArrayAllocation", ArrayAllocationTest),
                ["NestedArrays"] = () => RunTest("NestedArrays", NestedArraysTest),
                ["ObjectCreation"] = () => RunTest("ObjectCreation", ObjectCreationTest),
                ["DictionaryAccess"] = () => RunTest("DictionaryAccess", DictionaryAccessTest),
                ["RepeatedGC"] = () => RunTest("RepeatedGC", RepeatedGCTest),
                ["LargeHeap"] = () => RunTest("LargeHeap", LargeHeapTest),
                ["MixedWorkload"] = () => RunTest("MixedWorkload", MixedWorkloadTest),
                ["CLRContainerTraversal"] = () => RunTest("CLRContainerTraversal", CLRContainerTraversalTest),
                ["ToggleScenario"] = () => RunTest("ToggleScenario", ToggleScenarioTest),
            };
        }

        public List<BenchmarkResult> RunAllBenchmarks()
        {
            var results = new List<BenchmarkResult>();
            var testsToRun = _options.Tests?.Any() == true
                ? _tests.Where(t => _options.Tests.Contains(t.Key, StringComparer.OrdinalIgnoreCase))
                : _tests;

            foreach (var test in testsToRun)
            {
                Console.WriteLine($"Running: {test.Key}...");
                try
                {
                    var result = test.Value();
                    results.Add(result);
                    Console.WriteLine($"  Mean: {result.MeanMs:F3} ms, StdDev: {result.StdDevMs:F3} ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ERROR: {ex.Message}");
                    results.Add(new BenchmarkResult { TestName = test.Key + " (FAILED)" });
                }
            }

            return results;
        }

        private BenchmarkResult RunTest(string name, Action<Core, RuntimeCore> testAction)
        {
            var result = new BenchmarkResult { TestName = name };

            // Warmup
            for (int i = 0; i < _options.WarmupIterations; i++)
            {
                using (var env = new TestEnvironment())
                {
                    testAction(env.Core, env.RuntimeCore);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Actual runs
            for (int i = 0; i < _options.Iterations; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                var gcCountBefore = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
                var allocBefore = GC.GetTotalMemory(false);

                var sw = Stopwatch.StartNew();
                using (var env = new TestEnvironment())
                {
                    testAction(env.Core, env.RuntimeCore);
                }
                sw.Stop();

                var gcCountAfter = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
                var allocAfter = GC.GetTotalMemory(false);

                result.AddIteration(
                    sw.Elapsed.TotalMilliseconds,
                    gcCountAfter - gcCountBefore,
                    Math.Max(0, allocAfter - allocBefore));

                if (_options.Verbose)
                {
                    Console.WriteLine($"  Iteration {i + 1}: {sw.Elapsed.TotalMilliseconds:F3} ms");
                }
            }

            return result;
        }

        #region Test Methods

        /// <summary>
        /// Test simple variable allocations and GC.
        /// </summary>
        private void SimpleAllocationTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
a = 1;
b = 2;
c = a + b;
d = c * 2;
e = d + a;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test array allocation and GC.
        /// </summary>
        private void ArrayAllocationTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
arr1 = 1..100;
arr2 = arr1 * 2;
arr3 = arr1 + arr2;
sum = 0;
[Imperative]
{
    for (x in arr3)
    {
        sum = sum + x;
    }
}
arr1 = null;
arr2 = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test nested array allocation and GC.
        /// </summary>
        private void NestedArraysTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
nested = [];
[Imperative]
{
    for (i in 0..19)
    {
        inner = [];
        for (j in 0..19)
        {
            inner[j] = i * 20 + j;
        }
        nested[i] = inner;
    }
}
nested = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test FFI object creation and disposal.
        /// </summary>
        private void ObjectCreationTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
import(""FFITarget.dll"");
objs = [];
[Imperative]
{
    for (i in 0..49)
    {
        objs[i] = DummyPoint.ByCoordinates(i, i, i);
    }
}
objs = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test dictionary access patterns.
        /// </summary>
        private void DictionaryAccessTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
d = {""a"": 1, ""b"": 2, ""c"": 3, ""d"": 4, ""e"": 5};
keys = [""a"", ""b"", ""c"", ""d"", ""e""];
values = [];
[Imperative]
{
    for (i in 0..4)
    {
        values[i] = d[keys[i]];
    }
}
d = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test repeated explicit GC calls.
        /// </summary>
        private void RepeatedGCTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
[Imperative]
{
    for (i in 0..9)
    {
        temp = 1..100;
        temp = null;
        __GC();
    }
}
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test with large heap pressure.
        /// </summary>
        private void LargeHeapTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
large1 = 1..1000;
large2 = 1..1000;
large3 = large1 + large2;
large4 = large3 * 2;
result = large4[500];
large1 = null;
large2 = null;
large3 = null;
large4 = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test mixed workload with various operations.
        /// </summary>
        private void MixedWorkloadTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
// Primitives
a = 42;
b = 3.14;
c = ""hello"";

// Arrays
arr = 1..50;
arr2 = arr * 2;

// Nested
nested = [[1,2,3], [4,5,6], [7,8,9]];

// Dictionary
d = {""x"": arr, ""y"": nested};

// Access
val = d[""x""][25];
val2 = d[""y""][1][1];

// Cleanup
arr = null;
arr2 = null;
nested = null;
d = null;
__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test CLR container traversal (relevant to DYN-8717).
        /// This tests the GC's ability to trace through CLR-backed containers.
        /// </summary>
        private void CLRContainerTraversalTest(Core core, RuntimeCore runtimeCore)
        {
            string code = @"
import(""FFITarget.dll"");

// Create objects stored in dictionary
d = {
    ""p1"": DummyPoint.ByCoordinates(1, 2, 3),
    ""p2"": DummyPoint.ByCoordinates(4, 5, 6),
    ""p3"": DummyPoint.ByCoordinates(7, 8, 9)
};

// Access through dictionary
x1 = d[""p1""].X;
x2 = d[""p2""].X;
x3 = d[""p3""].X;

// Nested dictionary
nested = {
    ""inner"": {
        ""point"": DummyPoint.ByCoordinates(10, 11, 12)
    }
};

innerPoint = nested[""inner""][""point""];
xNested = innerPoint.X;

__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        /// <summary>
        /// Test the toggle scenario from DYN-8717.
        /// This simulates the True -> False -> True pattern that triggers the bug.
        /// </summary>
        private void ToggleScenarioTest(Core core, RuntimeCore runtimeCore)
        {
            // This test simulates multiple evaluation cycles with changing conditions,
            // which is similar to the DYN-8717 reproduction case.
            string code = @"
import(""FFITarget.dll"");

// First ""run"" - create dictionary with geometry
d = {""Point"": DummyPoint.ByCoordinates(1, 2, 3)};
result1 = d[""Point""].X;

// Simulate second ""run"" - different path, dictionary still exists
flag = false;
[Imperative]
{
    if (flag)
    {
        result2 = d[""Point""].Y;
    }
    else
    {
        result2 = ""no geometry"";
    }
}

// Force GC between ""runs""
__GC();

// Simulate third ""run"" - back to original path
flag = true;
[Imperative]
{
    if (flag)
    {
        // If GC incorrectly collected the Point, this would fail
        result3 = d[""Point""].Z;
    }
    else
    {
        result3 = ""no geometry"";
    }
}

__GC();
";
            ExecuteCode(code, core, runtimeCore);
        }

        #endregion

        private void ExecuteCode(string code, Core core, RuntimeCore runtimeCore)
        {
            var testFx = new ProtoTestFx.TD.TestFrameWork();
            testFx.RunScriptSource(code);
        }
    }

    /// <summary>
    /// Test environment that sets up ProtoCore for benchmarks.
    /// </summary>
    internal class TestEnvironment : IDisposable
    {
        public Core Core { get; }
        public RuntimeCore RuntimeCore { get; private set; }

        public TestEnvironment()
        {
            Core = new Core(new CoreOptions());
            Core.Compilers.Add(Language.Associative, new ProtoAssociative.Compiler(Core));
            Core.Compilers.Add(Language.Imperative, new ProtoImperative.Compiler(Core));
        }

        public void Dispose()
        {
            RuntimeCore?.Cleanup();
        }
    }
}
