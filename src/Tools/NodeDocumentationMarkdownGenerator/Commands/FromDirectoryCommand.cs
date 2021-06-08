using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    internal static class FromDirectoryCommand
    {
        /// <summary>
        /// Creates markdown files using the fromdirectory verb
        /// </summary>
        /// <param name="opts"></param>
        internal static void HandleDocumentationFromDirectory(FromDirectoryOptions opts)
        {
            var searchOption = opts.RecursiveScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var fileInfos = new List<MdFileInfo>();
            fileInfos.AddRange(ScanAssembliesFromOpts(opts.InputFolderPath, opts.Filter, opts.ReferencePaths, searchOption));
            if (opts.IncludeCustomNodes)
            {
                fileInfos.AddRange(ScanFolderForCustomNodes(opts.InputFolderPath, searchOption));
            }

            MarkdownHandler.CreateMdFilesFromFileNames(fileInfos, opts.OutputFolderPath, opts.Overwrite, opts.CompressImages, opts.DictionaryDirectory, opts.LayoutSpecPath);
        }

        private static List<MdFileInfo> ScanFolderForCustomNodes(string inputFolderPath, SearchOption searchOption)
        {
            var allDyfs = Directory.GetFiles(inputFolderPath, "*.dyf", searchOption).Select(x => new FileInfo(x)).ToList();
            var fileInfos = new List<MdFileInfo>();
            foreach (var cn in allDyfs)
            {
                var fileInfo = MdFileInfo.FromCustomNode(cn.FullName);
                if (fileInfo is null) continue;

                fileInfos.Add(fileInfo);
            }
            return fileInfos;
        }

        private static List<MdFileInfo> ScanAssembliesFromOpts(string inputFolderPath, IEnumerable<string> filter, IEnumerable<string> references, SearchOption searchOption)
        {
            var allDlls = Directory.GetFiles(inputFolderPath, "*.dll", searchOption).Select(x => new FileInfo(x)).ToList();

            var referencesDllPaths = references
                .SelectMany(p => new DirectoryInfo(p)
                    .EnumerateFiles("*.dll", SearchOption.AllDirectories)
                    .Select(d => d.FullName)
                    .ToList());

            if (filter.Count() != 0)
            {
                var dllPaths = allDlls
                    .Where(x => filter.Contains(x.Name) || filter.Contains(x.FullName))
                    .Select(x => x.FullName)
                    .ToList();

                var addtionalPathsToLoad = allDlls.Select(x => x.FullName).Except(dllPaths).ToList();
                if (referencesDllPaths.Count() > 0)
                {
                    addtionalPathsToLoad.AddRange(referencesDllPaths);
                }

                if (filter.Any(x=>x.EndsWith(".ds")))
                {
                    var dsFiles = Directory.GetFiles(inputFolderPath, "*.ds").Select(x => new FileInfo(x)).ToList();
                    dllPaths.AddRange(dsFiles
                        .Where(x => filter.Contains(x.Name) || filter.Contains(x.FullName))
                        .Select(x => x.FullName)
                        .ToList());
                }

                return AssemblyHandler.ScanAssemblies(dllPaths, addtionalPathsToLoad);
            }

            return AssemblyHandler.
                ScanAssemblies(allDlls.Select(x => x.FullName), referencesDllPaths);
        }
    }
}
