namespace DynamoPackagesAnalyzer.Models.DirectorySource
{
    /// <summary>
    /// represents the pkg.json file in a package
    /// </summary>
    internal class PkgJson
    {
        internal string License { get; set; }
        internal string file_hash { get; set; }
        internal string Name { get; set; }
        internal string Version { get; set; }
        internal string Description { get; set; }
        internal string Group { get; set; }
        internal string[] Keywords { get; set; }
        internal PkgDependency[] Dependencies { get; set; }
        internal string engine_version { get; set; }
        internal string engine { get; set; }
        internal string engine_metadata { get; set; }
        internal string site_url { get; set; }
        internal string repository_url { get; set; }
        internal bool contains_binaries { get; set; }
        internal string[] node_libraries { get; set; }
    }
}
