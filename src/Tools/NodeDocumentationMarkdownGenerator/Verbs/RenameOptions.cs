using CommandLine.Text;
using CommandLine;
using System.Collections.Generic;

namespace NodeDocumentationMarkdownGenerator.Verbs
{
    [Verb("rename", HelpText = "Renaming utilities for fallback MD files")]
    internal class RenameOptions
    {
        [Option('f', "file", HelpText = "Input MD file", Required = false)]
        public string InputMdFile { get; set; }
        [Option('d', "directory", HelpText = "Input directory", Required = false)]
        public string InputMdDirectory { get; set; }
        [Option('m', "maxlength", HelpText = "Max length before renaming to a shorter name", Required = false, Default = 50)]
        public int MaxLength { get; set; }
        [Usage(ApplicationAlias = "Dynamo docs generator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generate docs from package folder", new RenameOptions());
            }
        }
    }
}
