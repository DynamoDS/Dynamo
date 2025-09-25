using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests to validate that resource file paths don't exceed Windows path length limitations.
    /// This addresses the issue where CI servers with long path prefixes cause MSB3103 errors
    /// when resource paths become too long.
    /// </summary>
    [TestFixture]
    public class ResourcePathLengthTests : UnitTestBase
    {
        // Windows path length limitations
        private const int WINDOWS_MAX_PATH_LENGTH = 260; // Traditional limit
        
        // Estimated server prefix length to account for various CI systems
        // Based on: C:\Jenkins\workspace\Dynamo\DynamoSelfServe\pullRequestValidation\Dynamo\src
        // This is 82 characters, but using 120 to be safe for various CI systems
        private const int ESTIMATED_SERVER_PREFIX_LENGTH = 120;

        /// <summary>
        /// Result of analyzing a resource path
        /// </summary>
        public class ResourcePathAnalysis
        {
            public string ResourceName { get; set; }
            public string RelativePath { get; set; }
            public string FullPath { get; set; }
            public int EstimatedServerPathLength { get; set; }
            public int ActualPathLength { get; set; }
            public bool ExceedsWindowsLimit { get; set; }
            public string ResxFile { get; set; }
            public bool FileExists { get; set; }
        }

        /// <summary>
        /// Test that all .resx files have resource paths that won't exceed Windows path length limits on CI servers
        /// </summary>
        [Test]
        public void AllResourcePathsShouldNotExceedWindowsPathLengthLimits()
        {
            // Arrange
            var srcPath = Path.Combine(GetTestDirectory(), "..", "src");
            var problematicPaths = new List<ResourcePathAnalysis>();
            var allResults = new List<ResourcePathAnalysis>();
            var processedResxFiles = new List<string>();
            
            // Act - Analyze all .resx files
            var resxFiles = Directory.GetFiles(srcPath, "*.resx", SearchOption.AllDirectories);
            
            foreach (var resxFile in resxFiles)
            {
                var results = AnalyzeResxFile(resxFile);
                allResults.AddRange(results);
                problematicPaths.AddRange(results.Where(r => r.ExceedsWindowsLimit));
                
                // Track processed files (only add if it had resources)
                if (results.Any())
                {
                    processedResxFiles.Add(Path.GetRelativePath(srcPath, resxFile));
                }
            }

            // Assert - No paths should exceed Windows limits
            if (problematicPaths.Any())
            {
                var report = GeneratePathLengthReport(allResults, problematicPaths, resxFiles.Length, processedResxFiles);
                Assert.Fail($"Found {problematicPaths.Count} resource paths that exceed Windows path length limit of {WINDOWS_MAX_PATH_LENGTH} characters.\n\n{report}");
            }
            
            // Also verify we actually found some resources to test
            Assert.Greater(allResults.Count, 0, "Should have found some resource files to analyze");
        }

        /// <summary>
        /// Analyzes a .resx file for path length issues
        /// </summary>
        private List<ResourcePathAnalysis> AnalyzeResxFile(string resxFilePath)
        {
            var results = new List<ResourcePathAnalysis>();
            
            try
            {
                var doc = XDocument.Load(resxFilePath);
                var dataElements = doc.Root.Descendants("data")
                    .Where(d => d.Attribute("type")?.Value?.Contains("ResXFileRef") == true);

                foreach (var dataElement in dataElements)
                {
                    var nameAttr = dataElement.Attribute("name")?.Value;
                    var valueElement = dataElement.Element("value");
                    
                    if (nameAttr != null && valueElement != null)
                    {
                        var value = valueElement.Value;
                        var parts = value.Split(';');
                        if (parts.Length > 0)
                        {
                            var relativePath = parts[0];
                            var result = AnalyzeResourcePath(nameAttr, relativePath, resxFilePath);
                            results.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Error parsing .resx file {resxFilePath}: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Analyzes a specific resource path for length issues
        /// </summary>
        private ResourcePathAnalysis AnalyzeResourcePath(string resourceName, string relativePath, string resxFile)
        {
            var resxDirectory = Path.GetDirectoryName(resxFile);
            var actualPath = Path.GetFullPath(Path.Combine(resxDirectory, relativePath));
            
            // Calculate estimated server path length
            var serverPathLength = ESTIMATED_SERVER_PREFIX_LENGTH + relativePath.Length;

            return new ResourcePathAnalysis
            {
                ResourceName = resourceName,
                RelativePath = relativePath,
                FullPath = actualPath,
                EstimatedServerPathLength = serverPathLength,
                ActualPathLength = actualPath.Length,
                ExceedsWindowsLimit = serverPathLength > WINDOWS_MAX_PATH_LENGTH,
                ResxFile = resxFile,
                FileExists = File.Exists(actualPath)
            };
        }

        /// <summary>
        /// Generates a detailed report of path length issues
        /// </summary>
        private string GeneratePathLengthReport(List<ResourcePathAnalysis> allResults, List<ResourcePathAnalysis> problematicPaths, int totalResxFiles, List<string> processedResxFiles)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine($"=== RESOURCE PATH LENGTH ANALYSIS ===");
            report.AppendLine($"Total .resx files found: {totalResxFiles}");
            report.AppendLine($"Total .resx files with resources: {processedResxFiles.Count}");
            report.AppendLine($"Total resources analyzed: {allResults.Count}");
            report.AppendLine($"Resources exceeding Windows path limit ({WINDOWS_MAX_PATH_LENGTH}): {problematicPaths.Count}");
            report.AppendLine($"Estimated server prefix length: {ESTIMATED_SERVER_PREFIX_LENGTH} characters");
            report.AppendLine();
            
            report.AppendLine("=== .RESX FILES WITH RESOURCES ===");
            foreach (var resxFile in processedResxFiles.OrderBy(f => f))
            {
                report.AppendLine($"- {resxFile}");
            }
            report.AppendLine();

            if (problematicPaths.Any())
            {
                report.AppendLine("=== TOP 10 MOST PROBLEMATIC PATHS ===");
                var topProblematic = problematicPaths
                    .OrderByDescending(p => p.EstimatedServerPathLength)
                    .Take(10);

                foreach (var path in topProblematic)
                {
                    report.AppendLine($"Resource: {path.ResourceName}");
                    report.AppendLine($"  Estimated server path length: {path.EstimatedServerPathLength}");
                    report.AppendLine($"  Exceeds limit by: {path.EstimatedServerPathLength - WINDOWS_MAX_PATH_LENGTH} characters");
                    report.AppendLine($"  Relative path: {path.RelativePath}");
                    report.AppendLine($"  .resx file: {Path.GetFileName(path.ResxFile)}");
                    report.AppendLine($"  File exists: {path.FileExists}");
                    report.AppendLine();
                }

                // Analyze patterns
                var methodGroups = problematicPaths
                    .Where(p => p.ResourceName.Contains("-"))
                    .GroupBy(p => ExtractMethodName(p.ResourceName))
                    .OrderByDescending(g => g.Count())
                    .Take(5);

                if (methodGroups.Any())
                {
                    report.AppendLine();
                    report.AppendLine("=== MOST PROBLEMATIC METHOD PATTERNS ===");
                    foreach (var group in methodGroups)
                    {
                        report.AppendLine($"Method: {group.Key} - {group.Count()} problematic variants");
                        var maxLength = group.Max(p => p.EstimatedServerPathLength);
                        report.AppendLine($"  Max estimated path length: {maxLength}");
                    }
                }
            }

            return report.ToString();
        }

        /// <summary>
        /// Extracts method name from a resource name
        /// </summary>
        private string ExtractMethodName(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return "Unknown";
                
            var parts = resourceName.Split('.');
            if (parts.Length >= 5)
            {
                // For resources like "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildPipes.Curve1-double-..."
                // Extract the method part (BuildPipes)
                var methodPart = parts[5]; // Index 5 would be "BuildPipes" in the example
                var hyphenIndex = methodPart.IndexOf('-');
                return hyphenIndex > 0 ? methodPart.Substring(0, hyphenIndex) : methodPart;
            }
            
            return resourceName;
        }

        /// <summary>
        /// Helper method to get the test directory
        /// </summary>
        private string GetTestDirectory()
        {
            // This should point to the test directory structure
            return TestDirectory;
        }
    }
}
