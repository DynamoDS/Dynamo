using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    class FromDirectoryCommand
    {
        public FromDirectoryCommand()
        {
        }

        internal void HandleDocumentationFromDirectory(FromDirectoryOptions opts)
        {
            var searchOption = opts.RecursiveScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var fileInfos = new List<MdFileInfo>();
            fileInfos.AddRange(ScanAssembliesFromOpts(opts.InputFolderPath, opts.Filter, searchOption));
            if (opts.IncludeCustomNodes)
                fileInfos.AddRange(ScanFolderForCustomNodes(opts.InputFolderPath, searchOption));

            MarkdownHandler.CreateMdFilesFromFileNames(fileInfos, opts.OutputFolderPath, opts.Overwrite, opts.Dictionary);
        }

        private List<MdFileInfo> ScanFolderForCustomNodes(string inputFolderPath, SearchOption searchOption)
        {
            var allDyfs = Directory.GetFiles(inputFolderPath, "*.dyf", searchOption).Select(x => new FileInfo(x)).ToList();
            var fileInfos = new List<MdFileInfo>();
            foreach (var cn in allDyfs)
            {
                var fileInfo = MarkdownHandler.GetMdFileInfoFromFromCustomNode(cn.FullName);
                if (fileInfo is null)
                    continue;
                fileInfos.Add(fileInfo);
            }
            return fileInfos;
        }

        private List<MdFileInfo> ScanAssembliesFromOpts(string inputFolderPath, IEnumerable<string> filter, SearchOption searchOption)
        {
            var allDlls = Directory.GetFiles(inputFolderPath, "*.dll", searchOption).Select(x => new FileInfo(x)).ToList();

            if (filter.Count() != 0)
            {
                var dllPaths = allDlls.Where(x => filter.Contains(x.Name) || filter.Contains(x.FullName)).Select(x => x.FullName).ToList();
                return AssemblyHandler.ScanAssemblies(dllPaths);
            }

            return AssemblyHandler.
                ScanAssemblies(allDlls.
                Select(x => x.FullName).ToList());
        }
    }
}
