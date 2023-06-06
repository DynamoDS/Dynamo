namespace DynamoPackagesAnalyzer.Models.Greg
{
    /// <summary>
    /// Greg Client dto
    /// </summary>
    internal class PackageVersion
    {
        public string url_with_deps { get; set; }

        public string url { get; set; }

        public string contents { get; set; }

        public string engine_metadata { get; set; }

        public string engine_version { get; set; }

        public string created { get; set; }

        public List<string> full_dependency_versions { get; set; }

        public List<Dependency> full_dependency_ids { get; set; }

        public List<string> direct_dependency_versions { get; set; }

        public List<Dependency> direct_dependency_ids { get; set; }

        public IEnumerable<string> host_dependencies { get; set; }

        public string change_log { get; set; }

        public string version { get; set; }

        public bool contains_binaries { get; set; }

        public List<string> node_libraries { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public string copyright_holder { get; set; }

        public string copyright_year { get; set; }

        public string scan_status { get; set; }

        public string latest_version_update { get; set; }
    }
}
