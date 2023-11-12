using CommandLine;
using CommandLine.Text;

namespace DynamoPackagesAnalyzer.Models.CommandLine
{
    /// <summary>
    /// Command line arguments for directory mode
    /// </summary>
    [Verb("directory", HelpText = "Process every DLL in a given directory")]
    internal class DirectoryOptions
    {
        [Value(0, MetaName = "Directory", HelpText = "A local directory to the packages", Required = true)]
        internal string Path { get; set; }

        [Option('z', "ziparchives", Default = true, HelpText = "Defines whether analyze the zipped files in the given directory or to analyze an unzipped package directory")]
        internal bool HasZipArchives { get; set; }

        [Option('t', "multithread", Default = true, HelpText = "Defines whether to use multithread execution")]
        internal bool MultiThread { get; set; }

        [Option('l', "lookupdetails", Default = true, HelpText = "Whether to try lookup for package details at dynamopackages.com")]
        internal bool LookupDetails { get; set; }

        [Usage(ApplicationAlias = "DynamoAnalyzer")]
        internal static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new DirectoryOptions { Path = "some/path/to/dlls" });
                yield return new Example("Normal scenario", new DirectoryOptions { HasZipArchives = true, Path = "some/path/to/zipArchives" });
            }
        }

        internal string ArchiveName { get; set; }
    }
}
