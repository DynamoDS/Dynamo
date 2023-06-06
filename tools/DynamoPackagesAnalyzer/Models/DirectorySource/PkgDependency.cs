namespace DynamoPackagesAnalyzer.Models.DirectorySource
{
    /// <summary>
    /// represents the pkg.json file in a package
    /// </summary>
    internal class PkgDependency
    {
        public string name { get; set; }
        public string version { get; set; }
    }
}
