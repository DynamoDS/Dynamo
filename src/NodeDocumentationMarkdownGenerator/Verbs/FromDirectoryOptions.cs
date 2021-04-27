using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace NodeDocumentationMarkdownGenerator.Verbs
{
    [Verb("fromdirectory", HelpText = "Generate documentation from a directory containing binary files and dyfs")]
    internal class FromDirectoryOptions
    {

        [Option('i', "input", HelpText = "Directory folder path", Required = true)]
        public string InputFolderPath { get; set; }

        [Option('o', "output", HelpText = "Folder path to save generated documents in", Required = true)]
        public string OutputFolderPath { get; set; }

        [Option('f', "filter", HelpText = "Specifies which binary files documentation should be generated for", Required = false)]
        public IEnumerable<string> Filter { get; set; }

        [Option('c', "includedyfs", HelpText = "Include custom dyf nodes ....", Required = false, Default = true)]
        public bool IncludeCustomNodes { get; set; }

        [Option('d', "dictionary", HelpText = "Dictionary ...", Required = false)]
        public string Dictionary { get; set; }

        [Option('w', "overwrite", HelpText = "Overwrite ....", Required = false, Default = false)]
        public bool Overwrite { get; set; }

        [Option('p', "preview", HelpText = "Preview....", Required = false, Default = false)]
        public bool Preview { get; set; }

        [Option('r', "recursive-scan", HelpText = "Input folder will be scanned recursively", Required = false, Default = false)]
        public bool RecursiveScan { get; set; }

        [Usage(ApplicationAlias = "Dynamo docs generator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generate docs from package folder", new FromDirectoryOptions());
            }
        }

    }
}
