using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    internal class FromPackageFolderCommand
    {
        private readonly ILogger logger;

        public FromPackageFolderCommand(ILogger logger)
        {
            this.logger = logger;
        }

        internal void HandlePackageDocumentation(FromPackageOptions opts)
        {
            var package = PackageFromRoot(opts.InputFolderPath);

            var nodeLibraryFileNames = ScanNodeLibraries(package, opts.ReferencePaths);
            var customNodeFileNames = ScanCustomNodes(package);

            var fileInfos = new List<MdFileInfo>();
            fileInfos.AddRange(nodeLibraryFileNames);
            fileInfos.AddRange(customNodeFileNames);

            var outdir = package.NodeDocumentaionDirectory;
            if (!Directory.Exists(outdir))
            {
                Directory.CreateDirectory(outdir);
            }

            MarkdownHandler.CreateMdFilesFromFileNames(fileInfos, outdir, opts.Overwrite, logger);
        }

        private List<MdFileInfo> ScanNodeLibraries(Package pkg, IEnumerable<string> hostPaths)
        {
            var binDlls = new DirectoryInfo(pkg.BinaryDirectory)
                .EnumerateFiles("*.dll")
                .ToList();

            var nodeLibraryPaths = binDlls
                .Where(x => pkg.Header.node_libraries.Contains(AssemblyName.GetAssemblyName(x.FullName).FullName))
                .Select(x => x.FullName)
                .ToList();

            var addtionalPathsToLoad = binDlls.Select(x => x.FullName).Except(nodeLibraryPaths).ToList();

            var hostDllPaths = hostPaths
                .SelectMany(p => new DirectoryInfo(p)
                    .EnumerateFiles("*.dll", SearchOption.AllDirectories)
                    .Select(d => d.FullName)
                    .ToList());

            addtionalPathsToLoad.AddRange(hostDllPaths);

            return AssemblyHandler.ScanAssemblies(nodeLibraryPaths, logger, addtionalPathsToLoad);
        }

        private List<MdFileInfo> ScanCustomNodes(Package pkg)
        {
            var fileInfos = new List<MdFileInfo>();

            foreach (var path in Directory.EnumerateFiles(pkg.CustomNodeDirectory, "*.dyf"))
            {
                var fileInfo = MdFileInfo.FromCustomNode(path, logger);
                if (fileInfo is null) continue;

                fileInfos.Add(fileInfo);
            }
            return fileInfos;
        }

        private Package PackageFromRoot(string packageFolderPath)
        {
            var headerPath = Path.Combine(packageFolderPath, "pkg.json");
            Package pkg = Package.FromJson(headerPath, logger);

            return pkg;
        }
    }
}
