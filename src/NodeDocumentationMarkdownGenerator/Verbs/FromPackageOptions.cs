using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace NodeDocumentationMarkdownGenerator.Verbs
{
    [Verb("frompackage", HelpText = "Generate documentation from a package folder")]
    internal class FromPackageOptions
    {
        [Option('i', "input", HelpText = "Package folder path")]
        public string InputFolderPath { get; set; }

        [Option('r', "references", HelpText = "Folder paths to dlls that are used as references in the nodes", Required = false)]
        public IEnumerable<string> ReferencePaths { get; set; }

        [Option('d', "dictionary", HelpText = "Dictionary ...", Required = false)]
        public string Dictionary { get; set; }

        [Option('w', "overwrite", HelpText = "Overwrite ....", Required = false)]
        public bool Overwrite { get; set; }

        [Option('p', "preview", HelpText = "Preview....", Required = false)]
        public bool Preview { get; set; }

        [Usage(ApplicationAlias = "Dynamo docs generator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generate docs from package folder", new FromPackageOptions());
            }
        }
    }
}
