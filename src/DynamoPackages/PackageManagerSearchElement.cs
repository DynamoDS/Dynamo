using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dynamo.Search.SearchElements;
using Dynamo.Utilities;

using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class Compatibility
    {
        public CompatibilityDetail Dynamo { get; set; }
        public CompatibilityDetail Revit { get; set; }
        public CompatibilityDetail Civil3D { get; set; }
        public CompatibilityDetail DotNet { get; set; }
    }

    public class CompatibilityDetail
    {
        public List<string> Versions { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public Compatibility Compatibility { get; set; }
        public bool? IsCompatible { get; set; }

        public static bool? GetVersionCompatibility(List<VersionInfo> versionInfos, string packageVersion)
        {
            // Find the specific VersionInfo for the given package version
            var versionInfo = versionInfos?.FirstOrDefault(v => v.Version == packageVersion);

            // If no version info is found, return null (unknown compatibility)
            if (versionInfo == null)
            {
                return null;
            }

            return versionInfo.IsCompatible;
        }
    }

    /// <summary>
    /// A search element representing an element from the package manager </summary>
    public class PackageManagerSearchElement : SearchElementBase
    {
        #region Properties

        /// <summary>
        ///     An event that's invoked when the user has attempted to upvote this
        ///     package.
        /// </summary>
        public event Func<string, bool> UpvoteRequested;

        public string Maintainers { get { return String.Join(", ", this.Header.maintainers.Select(x => x.username)); } }
        private int _votes;
        public int Votes
        {
            get { return _votes; }
            set { _votes = value; RaisePropertyChanged("Votes"); }
        }
        public bool IsDeprecated { get { return this.Header.deprecated; } }
        public int Downloads { get { return this.Header.downloads; } }
        public string EngineVersion { get { return Header.versions[Header.versions.Count - 1].engine_version; } }
        public int UsedBy { get { return this.Header.used_by.Count; } }
        public string LatestVersion { get { return Header.versions != null ? Header.versions[Header.versions.Count - 1].version : String.Empty; } }
        public string LatestVersionCreated { get { return Header.versions[Header.versions.Count - 1].created; } }

        private VersionInfo latestCompatibleVersion;
        public VersionInfo LatestCompatibleVersion
        {
            get { return latestCompatibleVersion; }
            set
            {
                if (latestCompatibleVersion != value)
                {
                    latestCompatibleVersion = value;

                    RaisePropertyChanged(nameof(LatestCompatibleVersion));
                }
            }
        }




        public IEnumerable<string> PackageVersions { get { return Header.versions.OrderByDescending(x => x.version).Select(x => x.version); } }

        /// <summary>
        /// Hosts dependencies specified for latest version of particular package
        /// </summary>
        public List<string> Hosts { get { return Header.versions.Last().host_dependencies == null ? null : Header.versions.Last().host_dependencies.ToList(); } }

        /// <summary>
        /// Hosts dependencies string specified for latest version of particular package
        /// Used for package search element UI
        /// </summary>
        public string HostsString
        {
            get
            {
                var hostsString = String.Empty;
                if (Header.versions.Last().host_dependencies != null)
                {
                    foreach (var host in Header.versions.Last().host_dependencies)
                    {
                        hostsString += host + Environment.NewLine;
                    }
                }
                return hostsString.TrimEnd('\r', '\n');
            }
        }

        /// <summary>
        /// Header property </summary>
        /// <value>
        /// The PackageHeader used to instantiate this object </value>
        public PackageHeader Header { get; private set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public override string Type { get { return "Community Node"; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        public override string Name { get { return Header.name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        public override string Description { get { return Header.description ?? ""; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results </value>
        public override double Weight { get; set; }

        public override bool Searchable { get { return true; } }

        /// <summary>
        /// Guid property </summary>
        /// <value>
        /// A string that uniquely defines the CustomNodeDefinition </value>
        public Guid Guid { get; internal set; }

        /// <summary>
        /// Id property </summary>
        /// <value>
        /// A string that uniquely defines the Package on the server  </value>
        public string Id { get { return Header._id; } }

        public override string Keywords { get; set; }

        public string SiteUrl { get { return Header.site_url; } }
        public string RepositoryUrl { get { return Header.repository_url; } }

        public string InfectedPackageName { get; set; }
        public string InfectedPackageVersion { get; set; }
        public string InfectedPackageCreationDate { get; set; }

        private bool hasUpvote;
        /// <summary>
        ///     Shows if the current user has upvoted this package
        /// </summary>
        public bool HasUpvote
        {
            get
            {
                return hasUpvote;
            }

            internal set
            {
                hasUpvote = value;
                RaisePropertyChanged(nameof(HasUpvote));
            }
        }

        private List<VersionInfo> versionInfos;
        /// <summary>
        /// A detailed property for package Versions (+ compatibility info)
        /// </summary>
        public List<VersionInfo> VersionInfos
        {
            get { return versionInfos; }
            set
            {
                versionInfos = value;
                LatestCompatibleVersion = GetLatestCompatibleVersion();
                RaisePropertyChanged(nameof(VersionInfos));
            }
        }

        private VersionInfo GetLatestCompatibleVersion()
        {
            // Find the last compatible version
            var compatibleVersion = VersionInfos?.LastOrDefault(v => v.IsCompatible == true);

            // If no compatible version is found, return the latest version
            return compatibleVersion ?? VersionInfos?.LastOrDefault() ?? null;
        }


        #endregion

        /// <summary>
        ///     The class constructor
        /// </summary>
        /// <param name="header">The PackageHeader object describing the element</param>
        public PackageManagerSearchElement(PackageHeader header)
        {
            this.IsExpanded = false;
            this.Header = header;
            this.Weight = header.deprecated ? 0.1 : 1;

            if (header.keywords != null && header.keywords.Count > 0)
            {
                this.Keywords = String.Join(" ", header.keywords);
            }
            else
            {
                this.Keywords = "";
            }
            this.Votes = header.votes;
        }

        public PackageManagerSearchElement(PackageVersion infectedVersion)
        {
            this.InfectedPackageName = infectedVersion.name;
            this.InfectedPackageVersion = infectedVersion.version;
            this.InfectedPackageCreationDate = infectedVersion.created;
        }

        public void Upvote()
        {
            if (UpvoteRequested == null) return;

            Task<bool>.Factory.StartNew(() => UpvoteRequested(this.Id))
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        this.Votes += 1;
                    }
                }
                , TaskScheduler.FromCurrentSynchronizationContext());

            HasUpvote = true;
        }
    }
}
