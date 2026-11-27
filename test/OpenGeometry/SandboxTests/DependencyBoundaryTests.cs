using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class DependencyBoundaryTests
    {
        private static readonly string[] ForbiddenFilePrefixes =
        {
            "libg",
            "asm",
            "DynamoShapeManager",
            "usd_",
            "_omni_solid",
            "tbb"
        };

        private static readonly string[] ForbiddenImportPrefixes =
        {
            "asm",
            "libg",
            "usd_",
            "_omni_solid",
            "python",
            "tbb",
            "boost"
        };

        private static string RepoRoot =>
            Environment.GetEnvironmentVariable("OPEN_GEOMETRY_REPO_ROOT")
            ?? Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", ".."));

        private static string StagingDir =>
            Environment.GetEnvironmentVariable("OPEN_GEOMETRY_STAGING_DIR")
            ?? Path.Combine(RepoRoot, "bin", "open-geometry", "sandbox", "Release");

        [Test]
        public void WhenOpenGeometrySandboxRestoresThenAssetsContainNoLibGOrAsm()
        {
            var srcDir = Path.Combine(RepoRoot, "src");
            var assetFiles = Directory.GetFiles(srcDir, "project.assets.json", SearchOption.AllDirectories)
                .Where(p => p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}open-geometry{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.That(assetFiles, Is.Not.Empty, "Expected isolated obj/open-geometry/project.assets.json files.");

            var violations = new List<string>();
            bool foundDynamoCoreProjectRef = false;
            bool foundCoreNodesProjectRef = false;
            bool foundAnalysisProjectRef = false;
            bool foundGeometryColorProjectRef = false;
            bool foundTessellationProjectRef = false;

            foreach (var assetFile in assetFiles)
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(assetFile));
                var root = doc.RootElement;

                if (root.TryGetProperty("libraries", out var libraries))
                {
                    foreach (var lib in libraries.EnumerateObject())
                    {
                        if (ContainsForbiddenPackage(lib.Name))
                            violations.Add($"{assetFile}: libraries -> {lib.Name}");

                        if (lib.Name.StartsWith("ProtoGeometry/", StringComparison.OrdinalIgnoreCase))
                        {
                            var type = lib.Value.GetProperty("type").GetString();
                            var path = lib.Value.GetProperty("path").GetString();
                            if (!string.Equals(type, "project", StringComparison.OrdinalIgnoreCase)
                                || path == null
                                || !path.EndsWith("OpenGeometry/ProtoGeometry/ProtoGeometry.csproj", StringComparison.OrdinalIgnoreCase))
                            {
                                violations.Add($"{assetFile}: unexpected ProtoGeometry library ({type}: {path})");
                            }
                            else
                            {
                                if (assetFile.Contains($"{Path.DirectorySeparatorChar}DynamoCore{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                                    foundDynamoCoreProjectRef = true;
                                if (assetFile.Contains($"{Path.DirectorySeparatorChar}CoreNodes{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                                    foundCoreNodesProjectRef = true;
                                if (assetFile.Contains($"{Path.DirectorySeparatorChar}Analysis{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                                    foundAnalysisProjectRef = true;
                                if (assetFile.Contains($"{Path.DirectorySeparatorChar}GeometryColor{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                                    foundGeometryColorProjectRef = true;
                                if (assetFile.Contains($"{Path.DirectorySeparatorChar}Tesellation{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                                    foundTessellationProjectRef = true;
                            }
                        }
                    }
                }

                if (root.TryGetProperty("targets", out var targets))
                {
                    foreach (var tfm in targets.EnumerateObject())
                    {
                        foreach (var pkg in tfm.Value.EnumerateObject())
                        {
                            if (ContainsForbiddenPackage(pkg.Name))
                                violations.Add($"{assetFile}: targets[{tfm.Name}] -> {pkg.Name}");
                        }
                    }
                }
            }

            Assert.Multiple(() =>
            {
                Assert.That(violations, Is.Empty, "Found forbidden LibG/ASM/Autodesk ProtoGeometry restore assets.");
                Assert.That(foundDynamoCoreProjectRef, Is.True, "DynamoCore should reference the fork ProtoGeometry project.");
                Assert.That(foundCoreNodesProjectRef, Is.True, "CoreNodes should reference the fork ProtoGeometry project.");
                Assert.That(foundAnalysisProjectRef, Is.True, "Analysis should reference the fork ProtoGeometry project.");
                Assert.That(foundGeometryColorProjectRef, Is.True, "GeometryColor should reference the fork ProtoGeometry project.");
                Assert.That(foundTessellationProjectRef, Is.True, "Tessellation should reference the fork ProtoGeometry project.");
            });
        }

        [Test]
        public void WhenSandboxIsStagedThenNoAsmLibGOrAutodeskProtoGeometryFilesExist()
        {
            Assert.That(Directory.Exists(StagingDir), Is.True, $"Missing isolated staging directory: {StagingDir}");

            var requiredFiles = new[]
            {
                "DynamoSandbox.exe",
                "DynamoCore.dll",
                "DynamoCoreWpf.dll",
                "DynamoApplications.dll",
                "ProtoGeometry.dll",
                "ProtoGeometry.xml",
                "OpenGeometryBridge.dll",
                "SM_API.dll",
                "SMLib.dll",
                "BuiltIn.ds",
                "FunctionObject.ds",
                "DSCoreNodes.dll",
                "Analysis.dll",
                "GeometryColor.dll",
                "Tessellation.dll",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                Path.Combine("nodes", "CoreNodeModels.dll"),
                Path.Combine("nodes", "Watch3DNodeModels.dll")
            };

            var allEntries = Directory.EnumerateFileSystemEntries(StagingDir, "*", SearchOption.AllDirectories).ToArray();
            var forbiddenEntries = allEntries
                .Where(entry =>
                {
                    var name = Path.GetFileName(entry);
                    return string.Equals(name, "ProtoGeometry.config", StringComparison.OrdinalIgnoreCase)
                        || ForbiddenFilePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            Assert.Multiple(() =>
            {
                foreach (var rel in requiredFiles)
                {
                    Assert.That(File.Exists(Path.Combine(StagingDir, rel)), Is.True, $"Missing staged file: {rel}");
                }
                Assert.That(forbiddenEntries, Is.Empty, "Forbidden ASM/LibG/ShapeManager/USD/TBB files found in isolated staging directory.");
            });
        }

        [Test]
        public void WhenProtoGeometryAssemblyIsInspectedThenItIsTheForkBuild()
        {
            var protoPath = Path.Combine(StagingDir, "ProtoGeometry.dll");
            Assert.That(File.Exists(protoPath), Is.True);

            var name = AssemblyName.GetAssemblyName(protoPath);
            var versionInfo = FileVersionInfo.GetVersionInfo(protoPath);
            var metadata = ReadAssemblyMetadata(protoPath);

            Assert.Multiple(() =>
            {
                Assert.That(name.Name, Is.EqualTo("ProtoGeometry"));
                Assert.That(name.GetPublicKeyToken(), Is.Empty, "Fork ProtoGeometry.dll should not carry Autodesk's strong-name token.");
                Assert.That(versionInfo.ProductName, Is.EqualTo("OpenGeometry for Dynamo"));
                Assert.That(versionInfo.CompanyName, Is.EqualTo("OpenGeometry"));
                Assert.That(metadata.GetValueOrDefault("OpenGeometry.Kernel"), Is.EqualTo("SMLib"));
                Assert.That(metadata.GetValueOrDefault("OpenGeometry.Origin"), Is.EqualTo("open-geometry-dynamo"));
            });
        }

        [Test]
        public void WhenNativeDllImportsAreWalkedRecursivelyThenNoForbiddenDependenciesExist()
        {
            var roots = new[] { "OpenGeometryBridge.dll", "SM_API.dll", "SMLib.dll" };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(roots);
            var allImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (queue.Count > 0)
            {
                var dllName = queue.Dequeue();
                if (!visited.Add(dllName))
                    continue;

                var fullPath = Path.Combine(StagingDir, dllName);
                if (!File.Exists(fullPath))
                    continue;

                foreach (var imported in ReadPeImportedDlls(fullPath))
                {
                    allImports.Add(imported);
                    var stagedImport = Path.Combine(StagingDir, imported);
                    if (File.Exists(stagedImport) && !visited.Contains(imported))
                    {
                        queue.Enqueue(imported);
                    }
                }
            }

            var forbidden = allImports
                .Where(dll => ForbiddenImportPrefixes.Any(p => dll.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(dll => dll)
                .ToArray();

            TestContext.WriteLine("Recursive native imports: " + string.Join(", ", allImports.OrderBy(x => x)));

            Assert.Multiple(() =>
            {
                Assert.That(allImports, Does.Contain("SM_API.dll").IgnoreCase);
                Assert.That(allImports, Does.Contain("SMLib.dll").IgnoreCase);
                Assert.That(forbidden, Is.Empty, "Native geometry DLLs must not import ASM, LibG, OpenUSD, Python, or TBB.");
            });
        }

        private static bool ContainsForbiddenPackage(string packageKey)
        {
            return packageKey.Contains("LibG", StringComparison.OrdinalIgnoreCase)
                || packageKey.Contains("ASMAHL", StringComparison.OrdinalIgnoreCase)
                || packageKey.Contains("ShapeManager", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> ReadAssemblyMetadata(string assemblyPath)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new PEReader(stream);
            var reader = peReader.GetMetadataReader();

            foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
            {
                var attr = reader.GetCustomAttribute(handle);
                string attrTypeName = string.Empty;
                if (attr.Constructor.Kind == HandleKind.MemberReference)
                {
                    var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                    if (memberRef.Parent.Kind == HandleKind.TypeReference)
                    {
                        var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                        attrTypeName = reader.GetString(typeRef.Name);
                    }
                }

                if (attrTypeName != nameof(AssemblyMetadataAttribute))
                    continue;

                var blob = reader.GetBlobReader(attr.Value);
                if (blob.ReadUInt16() != 1)
                    continue;

                var key = blob.ReadSerializedString();
                var value = blob.ReadSerializedString();
                if (key != null)
                    result[key] = value ?? string.Empty;
            }

            return result;
        }

        private static IReadOnlyList<string> ReadPeImportedDlls(string pePath)
        {
            var imports = new List<string>();
            using var stream = File.OpenRead(pePath);
            using var peReader = new PEReader(stream);
            var headers = peReader.PEHeaders;
            var importDir = headers.PEHeader?.ImportTableDirectory ?? default;
            if (importDir.RelativeVirtualAddress == 0 || importDir.Size == 0)
                return imports;

            var imageBytes = peReader.GetEntireImage().GetContent();
            int offset = RvaToOffset(headers, importDir.RelativeVirtualAddress);
            if (offset < 0)
                return imports;

            while (offset + 20 <= imageBytes.Length)
            {
                int originalFirstThunk = BitConverter.ToInt32(imageBytes.AsSpan(offset, 4));
                int nameRva = BitConverter.ToInt32(imageBytes.AsSpan(offset + 12, 4));
                int firstThunk = BitConverter.ToInt32(imageBytes.AsSpan(offset + 16, 4));
                if (originalFirstThunk == 0 && nameRva == 0 && firstThunk == 0)
                    break;

                int nameOffset = RvaToOffset(headers, nameRva);
                if (nameOffset >= 0)
                {
                    int end = nameOffset;
                    while (end < imageBytes.Length && imageBytes[end] != 0)
                        end++;
                    imports.Add(Encoding.ASCII.GetString(imageBytes.AsSpan(nameOffset, end - nameOffset)));
                }

                offset += 20;
            }

            return imports;
        }

        private static int RvaToOffset(PEHeaders headers, int rva)
        {
            foreach (var section in headers.SectionHeaders)
            {
                int size = Math.Max(section.VirtualSize, section.SizeOfRawData);
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + size)
                {
                    return section.PointerToRawData + (rva - section.VirtualAddress);
                }
            }
            return -1;
        }
    }
}
