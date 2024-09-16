using CommandLine.Text;
using CommandLine;
using System.Collections.Generic;

namespace NodeDocumentationMarkdownGenerator.Verbs
{
    [Verb("rename", HelpText = "Renaming utilities for fallback MD files")]
    internal class RenameOptions
    {
        [Option('f', "file", HelpText = "Input MD file. Renames a single MD file including any support files to a shorter length (~56-60 characters) base file name.", Required = false)]
        public string InputMdFile { get; set; }
        [Option('d', "directory", HelpText = "Input directory. Inspects all MD files in a directory and renames all MD files with a base name longer that maxlength (see below).", Required = false)]
        public string InputMdDirectory { get; set; }
        [Option('m', "maxlength", HelpText = "Max length of the base file name before renaming to a shorter length (~56-60 characters) base file name.", Required = false, Default = 65)]
        public int MaxLength { get; set; }
        [Option('v', "verbose", HelpText = "Log more info, and save a log of renamed files to disk.", Required = false, Default = true)]
        public bool Verbose { get; set; }

        [Usage(ApplicationAlias = "Dynamo docs generator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Renaming utilities for fallback MD files", new RenameOptions());
            }
        }
    }
}
