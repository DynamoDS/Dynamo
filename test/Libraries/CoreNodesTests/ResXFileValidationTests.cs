using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class ResXFileValidationTests
    {
        private static readonly string ProjectRoot = GetProjectRoot();
        private static readonly string SrcPath = Path.Combine(ProjectRoot, "src");

        [Test]
        [Category("UnitTests")]
        public void AllResXReferencedPngFilesExistWithCorrectCase()
        {
            //Console.WriteLine($"Project Root: {ProjectRoot}");
            //Console.WriteLine($"Src Path: {SrcPath}");
            //Console.WriteLine($"Src Path Exists: {Directory.Exists(SrcPath)}");
            
            var errors = new List<string>();
            var resxFiles = FindAllResXFiles();

            //Console.WriteLine($"Found {resxFiles.Count} ResX files to check");

            foreach (var resxFile in resxFiles)
            {
                var pngReferences = ExtractPngReferences(resxFile);
                
                foreach (var pngRef in pngReferences)
                {
                    var validationResult = ValidatePngReference(resxFile, pngRef);
                    if (!validationResult.IsValid)
                    {
                        Console.WriteLine($"  ERROR: {validationResult.ErrorMessage}");
                        errors.Add(validationResult.ErrorMessage);
                    }
                    else
                    {
                        //Console.WriteLine($"  OK: {pngRef}");
                    }
                }
            }

            if (errors.Any())
            {
                var errorMessage = "ResX PNG file validation failed:\n" + string.Join("\n", errors);
                Assert.Fail(errorMessage);
            } else
            {
                Assert.Pass("All ResX PNG file references are valid.");
            }
        }

        private static List<FileInfo> FindAllResXFiles()
        {
            var resxFiles = new List<FileInfo>();
            
            if (Directory.Exists(SrcPath))
            {
                var allResxFiles = Directory.GetFiles(SrcPath, "*.resx", SearchOption.AllDirectories);
                resxFiles.AddRange(allResxFiles.Select(f => new FileInfo(f)));
            }

            return resxFiles;
        }

        private static List<string> ExtractPngReferences(FileInfo resxFile)
        {
            var pngReferences = new List<string>();
            
            try
            {
                var content = File.ReadAllText(resxFile.FullName);
                
                // Find all PNG file references using regex
                // Looking for patterns like: 
                // 1. ..\..\Resources\SomePath\SomeFile.png
                // 2. Resources\SomePath\SomeFile.png
                var matches = Regex.Matches(content, @"(?:\.\.\\\.\.\\|\.\.\\|)Resources\\[^""]*\.png", RegexOptions.IgnoreCase);
                
                foreach (Match match in matches)
                {
                    var relativePath = match.Value;
                    
                    // Remove the semicolon and everything after it (for .resx file references)
                    relativePath = relativePath.Split(';')[0];
                    
                    pngReferences.Add(relativePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read ResX file: {resxFile.FullName}", ex);
            }

            return pngReferences;
        }

        private static ValidationResult ValidatePngReference(FileInfo resxFile, string relativePath)
        {
            try
            {
                // Construct the full path relative to the .resx file's directory
                var resxDir = resxFile.Directory.FullName;
                var fullPath = Path.Combine(resxDir, relativePath);
                var expectedName = Path.GetFileName(relativePath);
                var directoryPath = Path.GetDirectoryName(fullPath);

                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"DIRECTORY NOT FOUND: {Path.GetDirectoryName(relativePath)} (referenced in {resxFile.Name})\n" +
                                     $"  Full path: {directoryPath}"
                    };
                }

                // Get all PNG files in the directory and find the one that matches case-insensitively
                var filesInDirectory = Directory.GetFiles(directoryPath, "*.png");
                var matchingFile = filesInDirectory.FirstOrDefault(f => 
                    string.Equals(Path.GetFileName(f), expectedName, StringComparison.OrdinalIgnoreCase));

                if (matchingFile == null)
                {
                    // File not found at all
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"FILE NOT FOUND: {relativePath} (referenced in {resxFile.Name})\n" +
                                     $"  Full path: {fullPath}\n" +
                                     $"  Directory contains: {string.Join(", ", filesInDirectory.Select(Path.GetFileName))}"
                    };
                }

                // Check if the actual filename matches the expected filename exactly
                var actualName = Path.GetFileName(matchingFile);
                if (expectedName != actualName)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"CASE MISMATCH: {relativePath} (referenced in {resxFile.Name})\n" +
                                     $"  Expected: {expectedName}\n" +
                                     $"  Actual:   {actualName}\n" +
                                     $"  Fix with: git mv \"{actualName}\" \"{expectedName}\""
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"ERROR validating {relativePath} (referenced in {resxFile.Name}): {ex.Message}"
                };
            }
        }

        private static string GetProjectRoot()
        {
            // Try multiple approaches to find the project root
            var currentDir = Directory.GetCurrentDirectory();
            
            // Approach 1: If we're in the test directory, go up to project root
            if (currentDir.Contains("test") && Directory.Exists(Path.Combine(currentDir, "src")))
            {
                return currentDir;
            }
            
            // Approach 2: Go up from test/Libraries/CoreNodesTests
            var testDir = Path.Combine(currentDir, "test");
            if (Directory.Exists(testDir))
            {
                return Path.GetDirectoryName(testDir);
            }
            
            // Approach 3: Navigate up the directory tree looking for src
            var dir = new DirectoryInfo(currentDir);
            while (dir != null)
            {
                var srcPath = Path.Combine(dir.FullName, "src");
                if (Directory.Exists(srcPath))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            
            // Approach 4: Try relative to the test assembly location
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);
            var projectRoot = Path.Combine(assemblyDir, "..", "..", "..", "..", "..");
            var normalizedPath = Path.GetFullPath(projectRoot);
            
            if (Directory.Exists(Path.Combine(normalizedPath, "src")))
            {
                return normalizedPath;
            }
            
            throw new InvalidOperationException($"Could not find project root directory. Current dir: {currentDir}, Assembly: {assemblyLocation}");
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
