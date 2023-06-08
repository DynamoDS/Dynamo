namespace DynamoPackagesAnalyzer.Models.DirectorySource
{
    /// <summary>
    /// represents the pkg.json file in a package
    /// </summary>
    internal class PkgDependency
    {
        internal string name { get; set; }
        internal string version { get; set; }
    }
}
