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
        public string Path { get; set; }

        [Option('z', "ziparchives", Default = true, HelpText = "Defines whether analyze the zipped files in the given directory or to analyze an unzipped package directory")]
        public bool HasZipArchives { get; set; }

        [Option('t', "multithread", Default = true, HelpText = "Defines whether to use multithread execution")]
        public bool MultiThread { get; set; }

        [Option('l', "lookupdetails", Default = true, HelpText = "Whether to try lookup for package details at dynamopackages.com")]
        public bool LookupDetails { get; set; }

        [Usage(ApplicationAlias = "DynamoAnalyzer")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new DirectoryOptions { Path = "some/path/to/dlls" });
                yield return new Example("Normal scenario", new DirectoryOptions { HasZipArchives = true, Path = "some/path/to/zipArchives" });
            }
        }

        public string ArchiveName { get; set; }
    }
}
