using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dynamo.Utilities;

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
        
        // Rename operation logging (similar to NodeDocumentationMarkdownGenerator pattern)
        private static List<string> RenameLog { get; set; } = new List<string>();
        
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
        /// Anything downstream of this method is just for reporting purposes
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
                
            // Remove size suffix (.Large or .Small) first
            var nameWithoutSize = resourceName;
            if (nameWithoutSize.EndsWith(".Large") || nameWithoutSize.EndsWith(".Small"))
            {
                var lastDotIndex = nameWithoutSize.LastIndexOf('.');
                nameWithoutSize = nameWithoutSize.Substring(0, lastDotIndex);
            }
            
            // Split by dots to get the parts
            var parts = nameWithoutSize.Split('.');
            
            if (parts.Length < 2)
                return nameWithoutSize;
            
            // Get the last part to check if it contains parameter specifications
            var lastPart = parts[parts.Length - 1];
            
            // Check if the last part looks like parameter specifications:
            // - Contains hyphens (e.g., "double-double-double")
            // - Is a common parameter type name (e.g., "double", "int", "Point", "Vector", etc.)
            // - Single parameter - is all lowercase (primitive types like "double", "int", "bool", "string")
            var hasParameterSpecs = lastPart.Contains("-") || 
                                   IsCommonParameterType(lastPart) ||
                                   IsAllLowercase(lastPart);
            
            if (hasParameterSpecs)
            {
                // Parameter specifications present, method name is second-to-last part
                return parts.Length >= 2 ? parts[parts.Length - 2] : parts[0];
            }
            else
            {
                // No parameter specifications, method name is the last part
                return lastPart;
            }
        }
        
        /// <summary>
        /// Checks if a string looks like a common parameter type
        /// </summary>
        private bool IsCommonParameterType(string part)
        {
            if (string.IsNullOrEmpty(part))
                return false;
                
            // Common parameter type names in Dynamo
            var commonTypes = new[] { "double", "int", "bool", "string", "Point", "Vector", 
                                    "Curve", "Surface", "Solid", "Geometry", "Line", "Circle",
                                    "Plane", "CoordinateSystem" };
            
            return commonTypes.Any(type => part.Equals(type, StringComparison.OrdinalIgnoreCase) ||
                                          part.StartsWith(type, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Checks if a string is all lowercase (likely a primitive parameter type)
        /// </summary>
        private bool IsAllLowercase(string part)
        {
            if (string.IsNullOrEmpty(part))
                return false;
                
            // Must have letters and all letters must be lowercase
            return part.Any(char.IsLetter) && part.All(c => !char.IsLetter(c) || char.IsLower(c));
        }

        /// <summary>
        /// ONE-TIME TEST: Renames problematic resource files to hash-based names and updates .resx references
        /// THIS METHOD SHOULD BE IGNORED AFTER RUNNING TO PREVENT ACCIDENTAL EXECUTION
        /// </summary>
        [Test]
        [Ignore("ONE-TIME USE ONLY - Comment out to run, then uncomment again")]
        public void RenameProblematicResourceFiles()
        {
            // Start new operation in log (append, don't clear)
            if (RenameLog.Any())
            {
                RenameLog.Add(""); // Empty line separator
            }
            RenameLog.Add($"Resource Rename Operation {DateTime.Now}");
            
            // Get all problematic paths
            var srcPath = Path.Combine(GetTestDirectory(), "..", "src");
            var problematicPaths = new List<ResourcePathAnalysis>();
            var resxFiles = Directory.GetFiles(srcPath, "*.resx", SearchOption.AllDirectories);
            
            foreach (var resxFile in resxFiles)
            {
                var results = AnalyzeResxFile(resxFile);
                problematicPaths.AddRange(results.Where(r => r.ExceedsWindowsLimit));
            }
            
            if (!problematicPaths.Any())
            {
                Assert.Inconclusive("No problematic paths found to rename.");
                return;
            }

            RenameLog.Add($"Found {problematicPaths.Count} problematic resource paths to rename");
            
            // Group by .resx file to process each file once
            var groupedByResx = problematicPaths.GroupBy(p => p.ResxFile);
            
            foreach (var resxGroup in groupedByResx)
            {
                RenameResourceFilesForResx(resxGroup.Key, resxGroup.ToList());
            }
            
            RenameLog.Add("Renaming operation completed successfully");
            
            // Write log file(s) - one per .resx file processed
            WriteRenameLogFiles(groupedByResx.Select(g => g.Key));
        }
        
        /// <summary>
        /// Renames problematic resource files for a specific .resx file and updates the .resx references
        /// </summary>
        private void RenameResourceFilesForResx(string resxFile, List<ResourcePathAnalysis> problematicResources)
        {
            var resxDirectory = Path.GetDirectoryName(resxFile);
            var renamedFiles = new Dictionary<string, string>(); // oldName -> newName
            
            RenameLog.Add($"Processing {Path.GetFileName(resxFile)}");
            
            // Step 1: Rename physical files
            foreach (var resource in problematicResources)
            {
                if (resource.FileExists)
                {
                    var oldFileName = Path.GetFileName(resource.RelativePath);
                    var baseName = Path.GetFileNameWithoutExtension(oldFileName);
                    var extension = Path.GetExtension(oldFileName);
                    
                    // Generate hash name using same method as existing RenameCommand
                    var hashName = Dynamo.Utilities.Hash.GetHashFilenameFromString(baseName);
                    var newFileName = hashName + extension;
                    
                    var oldFilePath = Path.GetFullPath(Path.Combine(resxDirectory, resource.RelativePath));
                    var newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), newFileName);
                    
                    if (File.Exists(oldFilePath))
                    {
                        File.Move(oldFilePath, newFilePath);
                        renamedFiles[oldFileName] = newFileName;
                        RenameLog.Add($"renamed {baseName}: {hashName}");
                    }
                    else
                    {
                        RenameLog.Add($"WARNING: File not found: {oldFilePath}");
                    }
                }
            }
            
            // Step 2: Update .resx file references
            if (renamedFiles.Any())
            {
                UpdateResxFileReferences(resxFile, renamedFiles);
                RenameLog.Add($"Updated {renamedFiles.Count} references in {Path.GetFileName(resxFile)}");
            }
        }
        
        /// <summary>
        /// Updates .resx file to reference the new hash-named files
        /// </summary>
        private void UpdateResxFileReferences(string resxFile, Dictionary<string, string> renamedFiles)
        {
            var content = File.ReadAllText(resxFile);
            var originalContent = content;
            
            foreach (var rename in renamedFiles)
            {
                var oldFileName = rename.Key;
                var newFileName = rename.Value;
                
                // Replace the file reference in the .resx content
                content = content.Replace(oldFileName, newFileName);
            }
            
            // Only write if changes were made
            if (content != originalContent)
            {
                File.WriteAllText(resxFile, content);
            }
        }
        
        /// <summary>
        /// Writes rename log files following the NodeDocumentationMarkdownGenerator pattern
        /// </summary>
        private void WriteRenameLogFiles(IEnumerable<string> processedResxFiles)
        {
            foreach (var resxFile in processedResxFiles)
            {
                var resxDirectory = Path.GetDirectoryName(resxFile);
                var logPath = Path.Combine(resxDirectory, "resource_rename_log.txt");
                var logString = string.Join(Environment.NewLine, RenameLog);
                
                if (File.Exists(logPath))
                {
                    File.AppendAllText(logPath, Environment.NewLine + logString);
                }
                else
                {
                    File.WriteAllText(logPath, logString);
                }
            }
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
