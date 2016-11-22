using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF.Models
{
    public class Dependency
    {
        public string name { get; set; }

        public string _id { get; set; }
    }

    public class PackageVersion
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

        public string change_log { get; set; }

        public string version { get; set; }

        public bool contains_binaries { get; set; }

        public List<string> node_libraries { get; set; }
    }

    public class User
    {
        public string username { get; set; }

        public string _id { get; set; }
    }

    public class TermsOfUseStatus
    {
        public string user_id { get; set; }
        public Boolean accepted { get; set; }
    }

    public class Comment
    {
        public string text { get; set; }
        public string user { get; set; }
        public string created { get; set; }
    }

    public class PackageHeader
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

        public int num_dependents { get; set; }

        public string description { get; set; }

        public List<User> maintainers { get; set; }

        public List<string> keywords { get; set; }

        public bool white_list { get; set; }

    }

    public class PackageDependency
    {
        public PackageDependency(string name, string version)
        {
            this.name = name;
            this.version = version;
        }
        public string name { get; set; }
        public string version { get; set; }
    }

    public class PackageUploadRequestBody 
    {
        internal PackageUploadRequestBody()
        {

        }

        public PackageUploadRequestBody(string name, string version, string description,
            IEnumerable<string> keywords, string license, string contents, string engine, string engineVersion,
            string metadata, string group, IEnumerable<PackageDependency> dependencies,
            string siteUrl, string repositoryUrl, bool containsBinaries,
            IEnumerable<string> nodeLibraryNames, string assetID=null)
        {
            this.name = name;
            this.version = version;
            this.description = description;
            this.dependencies = dependencies;
            this.keywords = keywords;
            this.contents = contents;
            this.engine = engine;
            this.group = group;
            this.engine_version = engineVersion;
            this.engine_metadata = metadata;
            this.site_url = siteUrl;
            this.repository_url = repositoryUrl;
            this.contains_binaries = containsBinaries;
            this.node_libraries = nodeLibraryNames;
            this.license = license;
            this.AssetID = assetID;
        }

        public string file_hash { get; set; }

        public string name { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public string group { get; set; }
        public IEnumerable<string> keywords { get; set; }
        public IEnumerable<PackageDependency> dependencies { get; set; }
        public string contents { get; set; }
        public string engine_version { get; set; }
        public string engine { get; set; }
        public string engine_metadata { get; set; }
        public string site_url { get; set; }
        public string repository_url { get; set; }
        public bool contains_binaries { get; set; }
        public IEnumerable<string> node_libraries { get; set; }

        public string license { get; set; }
        public string AssetID { get; set; }
    }
}
