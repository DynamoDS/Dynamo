namespace DynamoPackagesAnalyzer.Models.Greg
{
    /// <summary>
    /// Greg Client dto
    /// </summary>
    internal class PackageHeader
    {
        public string _id { get; set; }

        public string name { get; set; }

        public List<PackageVersion> versions { get; set; }

        public DateTime latest_version_update { get; set; }

        public int num_versions { get; set; }

        public List<Comment> comments { get; set; }

        public int num_comments { get; set; }

        public string latest_comment { get; set; }

        public int votes { get; set; }

        public int downloads { get; set; }

        public string repository_url { get; set; }

        public string site_url { get; set; }

        public bool banned { get; set; }

        public bool deprecated { get; set; }

        public string group { get; set; }

        public string engine { get; set; }

        public string license { get; set; }

        public List<Dependency> used_by { get; set; }

        public List<string> host_dependencies { get; set; }

        public int num_dependents { get; set; }

        public string description { get; set; }

        public List<User> maintainers { get; set; }

        public List<string> keywords { get; set; }
        /// <summary>
        /// Used to order the dlls when saving to CSV
        /// </summary>
        public int Index { get; set; }
    }
}
