using CommandLine;
using CommandLine.Text;

namespace DynamoPackagesAnalyzer.Models.CommandLine
{
    /// <summary>
    /// Command line arguments for zipfile mode
    /// </summary>
    [Verb("zipfile", HelpText = "Process a zip package file")]
    internal class ZipArchiveOptions
    {
        [Option('f', "files", Required = true, HelpText = "The files to be processed")]
        internal IEnumerable<string> Files { get; set; }

        [Option('t', "multithread", Default = true, HelpText = "Defines whether to use multithread execution")]
        internal bool MultiThread { get; set; }

        [Option('l', "lookupdetails", Default = true, HelpText = "Whether to try lookup for package details at dynamopackages.com")]
        internal bool LookupDetails { get; set; }

        [Usage(ApplicationAlias = "DynamoAnalyzer")]
        internal static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new ZipArchiveOptions { Files = new string[] { "some/path/to/zipFile" }, LookupDetails = true });
            }
        }
    }
}
