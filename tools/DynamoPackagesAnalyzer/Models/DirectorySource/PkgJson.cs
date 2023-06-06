namespace DynamoPackagesAnalyzer.Models.DirectorySource
{
    /// <summary>
    /// represents the pkg.json file in a package
    /// </summary>
    internal class PkgJson
    {
        public string License { get; set; }
        public string file_hash { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string[] Keywords { get; set; }
        public PkgDependency[] Dependencies { get; set; }
        public string engine_version { get; set; }
        public string engine { get; set; }
        public string engine_metadata { get; set; }
        public string site_url { get; set; }
        public string repository_url { get; set; }
        public bool contains_binaries { get; set; }
        public string[] node_libraries { get; set; }
    }
}
