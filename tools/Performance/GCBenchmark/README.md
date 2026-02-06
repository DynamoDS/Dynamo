# GC Benchmark Tool

A performance benchmark tool for measuring DesignScript garbage collection performance.

## Purpose

This tool measures GC performance metrics to:
1. Establish baseline performance before changes
2. Verify no significant regressions after changes
3. Compare different GC implementations

## Building

```bash
# From the Dynamo repository root
dotnet build tools/Performance/GCBenchmark/GCBenchmark.csproj -c Release
```

## Usage

```bash
# Run all benchmarks with default settings (10 iterations)
./GCBenchmark.exe

# Run with more iterations for better statistical accuracy
./GCBenchmark.exe -i 20

# Run specific tests
./GCBenchmark.exe -t SimpleAllocation,ArrayAllocation,ToggleScenario

# Output results to markdown file
./GCBenchmark.exe -o results.md

# Verbose output
./GCBenchmark.exe -v
```

## Command Line Options

| Option | Default | Description |
|--------|---------|-------------|
| `-i, --iterations` | 10 | Number of iterations per test |
| `-w, --warmup` | 2 | Number of warmup iterations |
| `-o, --output` | none | Output file path (markdown format) |
| `-v, --verbose` | false | Enable verbose output |
| `-t, --tests` | all | Specific tests to run (comma-separated) |

## Available Tests

| Test | Description |
|------|-------------|
| `SimpleAllocation` | Basic variable allocation and GC |
| `ArrayAllocation` | Array creation and disposal |
| `NestedArrays` | Multi-dimensional array handling |
| `ObjectCreation` | FFI object creation and disposal |
| `DictionaryAccess` | Dictionary operations and GC |
| `RepeatedGC` | Multiple explicit GC cycles |
| `LargeHeap` | Large allocation pressure |
| `MixedWorkload` | Combined operations |
| `CLRContainerTraversal` | CLR-backed container traversal (DYN-8717 related) |
| `ToggleScenario` | Toggle pattern that triggers DYN-8717 |

## Output Format

Results are displayed in both console and markdown format:

```
=== Results ===

Test                                          Mean (ms)     StdDev        Min        Max   GC Count
----------------------------------------------------------------------------------------------------
SimpleAllocation                                  1.234      0.123      1.100      1.450        2.0
ArrayAllocation                                   5.678      0.456      5.200      6.100        3.0
...
```

## Interpreting Results

- **Mean (ms)**: Average execution time across all iterations
- **StdDev**: Standard deviation (lower is more consistent)
- **Min/Max**: Range of execution times
- **GC Count**: Average number of .NET GC cycles triggered

## DYN-8717 Specific Tests

The `CLRContainerTraversal` and `ToggleScenario` tests are specifically designed to exercise the code paths relevant to the DYN-8717 GC bug:

- Tests dictionary access with FFI objects
- Tests nested container traversal
- Tests the True -> False -> True toggle pattern

After implementing the fix for DYN-8717, these tests should show:
- No significant performance regression for most scenarios (typically < 5% slowdown)
- For CLR-backed container traversal scenarios (for example, `CLRContainerTraversal`), an increased overhead of approximately 12–18% slowdown is expected and acceptable due to the additional GC work introduced by the fix
- Consistent execution times (low StdDev)
