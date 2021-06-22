using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace NodeDocumentationMarkdownGenerator.Verbs
{
    [Verb("fromdirectory", HelpText = "Generate documentation from a directory containing binary files and dyfs")]
    internal class FromDirectoryOptions
    {

        [Option('i', "input", HelpText = "Directory folder path containing assemblies to scan for nodes and create documentation files for", Required = true)]
        public string InputFolderPath { get; set; }

        [Option('o', "output", HelpText = "Folder path to save generated documents in", Required = true)]
        public string OutputFolderPath { get; set; }

        [Option('r', "references", HelpText = "Folder paths to directories containing binaries that are used as references in the nodes", Required = false)]
        public IEnumerable<string> ReferencePaths { get; set; }

        [Option('f', "filter", HelpText = "Specifies which binary files documentation should be generated for", Required = false)]
        public IEnumerable<string> Filter { get; set; }

        [Option('c', "includedyfs", HelpText = "Include custom dyf nodes", Required = false, Default = true)]
        public bool IncludeCustomNodes { get; set; }

        [Option('d', "dictionary", HelpText = "File path to DynamoDictionary json", Required = false)]
        public string DictionaryDirectory { get; set; }

        [Option('w', "overwrite", HelpText = "When specified the tool will overwrite files in the output path", Required = false, Default = false)]
        public bool Overwrite { get; set; }

        [Option('y', "recursive-scan", HelpText = "Input folder will be scanned recursively", Required = false, Default = false)]
        public bool RecursiveScan { get; set; }

        [Option('s', "compress-images", HelpText = "When set, the tool will try to compress images from dictionary content", Required = false, Default = false)]
        public bool CompressImages { get; set; }

        [Option('x', "layout-spec", HelpText = "Path to a LayoutSpecification json file", Required = false)]
        public string LayoutSpecPath { get; set; }

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
