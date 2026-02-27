using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using DynamoServices;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class VersionInformation
    {
        /// <summary>
        /// The version number as string
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Indicates whether the version is compatible (true, false, or unknown)
        /// </summary>
        public bool? IsCompatible { get; set; }

        /// <summary>
        /// Indicates whether this version is currently installed locally.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// A helper method to determine the compatibility of a specific package version from the provided version details.
        /// </summary>
        /// <param name="versionDetails">A collection of VersionInformation </param>
        /// <param name="packageVersion">The version to look for</param>
        /// <returns></returns>
        public static bool? GetVersionCompatibility(List<VersionInformation> versionDetails, string packageVersion)
        {
            // Find the specific VersionInformation for the given package version
            var versionInformation = versionDetails?.FirstOrDefault(v => v.Version == packageVersion);

            // If no version info is found, return null (unknown compatibility)
            if (versionInformation == null)
            {
                return null;
            }

            return versionInformation.IsCompatible;
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

        private VersionInformation latestCompatibleVersion;
        /// <summary>
        /// The latest compatible package version, or latest version if no compatible version is found
        /// </summary>
        public VersionInformation LatestCompatibleVersion
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

        /// <summary>
        /// A list of all package versions
        /// </summary>
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

        private List<VersionInformation> versionDetails;
        /// <summary>
        /// A property for package Versions details (+ compatibility info)
        /// </summary>
        public List<VersionInformation> VersionDetails
        {
            get { return versionDetails; }
            set
            {
                versionDetails = value;
                LatestCompatibleVersion = GetLatestCompatibleVersion();
                RaisePropertyChanged(nameof(VersionDetails));
            }
        }

        /// <summary>
        /// The currently selected version - need to pass onto the PackageDetailView
        /// </summary>
        public VersionInformation SelectedVersion { get; internal set; }

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
            this.VersionDetails = TransformVersionsToVersionInformation(header);
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

        /// <summary>
        /// Transforms package versions into VersionInformation objects, calculating compatibility if available.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private List<VersionInformation> TransformVersionsToVersionInformation(Greg.Responses.PackageHeader header)
        {
            var versionInformation = new List<VersionInformation>();
            if (header.versions == null) return versionInformation;
            var name = header.name;

            // Iterate through each version entry in the header
            foreach (var versionEntry in header.versions)
            {
                var compatibilityMatrix = versionEntry.compatibility_matrix;
                var hasCompatibilityMatrix = compatibilityMatrix != null;

                var vInformation = new VersionInformation
                {
                    Version = versionEntry.version, // Map the version directly
                    IsCompatible = hasCompatibilityMatrix ? CalculateCompatibility(compatibilityMatrix) : null // If no compatibility matrix, set IsCompatible to null (Unknown Compatibility)
                };

                versionInformation.Add(vInformation);
            }

            return versionInformation;
        }


        /// <summary>
        /// Determines if a package version is compatible based on the current Dynamo/Host and the compatibility matrix
        /// </summary>
        /// <param name="compatibilityMatrix">The compatibility matrix containing the user-defined compatibility information</param>
        /// <param name="dynamoVersion">Optional (for testing) - provide dynamo version</param>
        /// <param name="hostVersion">Optional (for testing) - provide host version</param>
        /// <param name="host">Optional (for testing) - provide host name</param>
        /// <param name="map">Optional (for testing) - the compatibility map</param>
        /// <returns></returns>
        internal static bool? CalculateCompatibility(List<Greg.Responses.Compatibility> compatibilityMatrix, Version dynamoVersion = null, Dictionary<string, Dictionary<string, string>> map = null, Version hostVersion = null, string host = null)
        {
            try
            {
                // Set defaults if parameters are not provided
                dynamoVersion ??= VersionUtilities.Parse(DynamoModel.Version);
                hostVersion ??= DynamoModel.HostAnalyticsInfo.HostProductVersion;
                host ??= DynamoModel.HostAnalyticsInfo.HostProductName;
                host = host?.ToLowerInvariant();

                // If there is no compatibility matrix, we cannot determine anything
                if (compatibilityMatrix == null || compatibilityMatrix.Count == 0)
                {
                    return null;
                }

                // If there is any mal-formed data, we reject
                if (compatibilityMatrix.Any(x => string.IsNullOrEmpty(x.name)))
                {
                    return null;
                }

                Greg.Responses.Compatibility compatibility = null;

                // Determine compatibility
                // 1. If we are under host, we first look at that host compatibility. If that's missing, we still check for Dynamo-only compatibility
                // 2. If we are not under host, we care about Dynamo compatibility first and foremost. However, we use fallback to Host compatibility if no Dynamo compatibility is found. In this case, we mark as Incompatible
                // 3. Only if no compatibility information is found, then we mark as 'Unknown Compatibility'
                if (!string.IsNullOrEmpty(host))
                {
                    // Check for host-specific compatibility
                    compatibility = compatibilityMatrix.FirstOrDefault(c => c.name?.ToLowerInvariant() == host?.ToLowerInvariant())
                                    ?? compatibilityMatrix.FirstOrDefault(c => c.name?.ToLowerInvariant() == "dynamo");

                    //If we have no compatibility that means didn't match the supported cases 
                    if (compatibility == null)
                    {
                        return false;
                    }
                }
                else
                {
                    // No host specified (DynamoCore only)
                    // Check Dynamo compatibility first; if missing, check for host compatibility
                    compatibility = compatibilityMatrix.FirstOrDefault(c => c.name?.ToLowerInvariant() == "dynamo");

                    if (compatibility == null)
                    {
                        // No Dynamo compatibility, fallback to any host compatibility if present
                        var hostCompatibility = compatibilityMatrix.FirstOrDefault(c => c.name?.ToLowerInvariant() != "dynamo");
                        if (hostCompatibility != null)
                        {
                            // Mark as incompatible if host compatibility is present without Dynamo info
                            return false;
                        }
                    }
                }

                // If no compatibility information is found for both Dynamo and host, return unknown (null)
                if (compatibility == null)
                {
                    return null;
                }

                // Check compatibility based on min/max ranges or specific versions
                var versionToCheck = (compatibility.name.ToLowerInvariant() == "dynamo") ? dynamoVersion : hostVersion;
                return versionToCheck != null && IsVersionCompatible(compatibility, versionToCheck);
            }
            catch (Exception ex)
            {
                DynamoConsoleLogger.OnLogMessageToDynamoConsole(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Helper function to validate version compatibility using min, max, and versions array.
        /// Supports min-only ranges, (in the future: max with asterisk), and schema validation rules.
        /// </summary>
        internal static bool IsVersionCompatible(Greg.Responses.Compatibility compatibility, Version version)
        {
            // Start by trimming away the Revision value 
            version = VersionUtilities.NormalizeVersion(version);

            // Check if the version is explicitly listed
            bool isListedInVersions = NormalizeAndContain(compatibility.versions, version);

            // Parse min and max values, if provided, and check for valid range
            bool isWithinMinMax = false;
            if (!string.IsNullOrEmpty(compatibility.min) || !string.IsNullOrEmpty(compatibility.max))
            {
                Version minVersion = VersionUtilities.Parse(compatibility.min);
                Version maxVersion = VersionUtilities.Parse(compatibility.max);

                // if max is null, try to parse based on wildcard semantics
                if(maxVersion == null)
                {
                    maxVersion = VersionUtilities.WildCardParse(compatibility.max);
                }

                // If max is specified without min, return false as max-only ranges are unsupported
                if (minVersion == null && maxVersion != null)
                {
                    return false;
                }

                // Validate that min is less than or equal to max if both are specified
                if (minVersion != null && maxVersion != null && minVersion > maxVersion)
                {
                    throw new ArgumentException("Min version cannot be greater than max version.");
                }

                // Check min-only case: compatibility only extends to the same major version
                if (minVersion != null && maxVersion == null)
                {
                    isWithinMinMax = version >= minVersion && version.Major == minVersion.Major;
                }
                else
                {
                    // Check within min and max bounds for cases where both are specified
                    isWithinMinMax = (minVersion == null || version >= minVersion) &&
                                     (maxVersion == null || version <= maxVersion);
                }
            }

            return isListedInVersions || isWithinMinMax;
        }

        // Aligns Dynamo's fully-defined version with Compatibility Matrix partial version strings
        internal static bool NormalizeAndContain(IEnumerable<string> versionStrings, Version version)
        {
            if (versionStrings == null) return false;

            // Normalize the input version to a 3-part format (Major.Minor.Build)
            var normalizedVersion = VersionUtilities.NormalizeVersion(version);

            foreach (var v in versionStrings)
            {
                // Parse and normalize each version string
                var parsedVersion = VersionUtilities.Parse(v);
                if (parsedVersion != null)
                {
                    var normalizedParsedVersion = VersionUtilities.NormalizeVersion(parsedVersion);

                    // Compare the normalized versions
                    if (normalizedVersion.Equals(normalizedParsedVersion))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Method to find Dynamo compatibility based on other hosts in the compatibilityMatrix
        internal static Greg.Responses.Compatibility GetDynamoCompatibilityFromHost(List<Greg.Responses.Compatibility> compatibilityMatrix, Dictionary<string, Dictionary<string, string>> map = null)
        {
            var compatibilityMap = PackageManagerClient.GetCompatibilityMap();
            if (DynamoModel.IsTestMode)
            {
                compatibilityMap = map;
            }

            // Check if the static compatibility map is null or empty
            if (compatibilityMap == null || compatibilityMap.Count == 0)
            {
                throw new InvalidOperationException("The compatibility map is not initialized.");
            }

            foreach (var hostCompatibility in compatibilityMatrix)
            {
                // Skip if it's explicitly Dynamo, we're looking for hosts other than Dynamo
                if (hostCompatibility.name == "Dynamo")
                    continue;

                // Check if the static compatibility map contains the host
                if (compatibilityMap.ContainsKey(DynamoUtilities.StringUtilities.CapitalizeFirstLetter(hostCompatibility.name)))
                {
                    var matchingHost = compatibilityMap[DynamoUtilities.StringUtilities.CapitalizeFirstLetter(hostCompatibility.name)];

                    string matchingMin = null;
                    string matchingMax = null;

                    // Only try to get min/max if they are not null
                    if (hostCompatibility.min != null)
                    {
                        matchingHost.TryGetValue(hostCompatibility.min.ToString(), out matchingMin);
                    }

                    if (hostCompatibility.max != null)
                    {
                        matchingHost.TryGetValue(hostCompatibility.max.ToString(), out matchingMax);
                    }

                    var dynamoVersions = new List<string>();

                    // If specific versions are provided, map them directly to their corresponding Dynamo versions
                    if (hostCompatibility.versions != null && hostCompatibility.versions.Any())
                    {
                        foreach (var hostVersion in hostCompatibility.versions)
                        {
                            if (matchingHost.TryGetValue(hostVersion, out var dynamoVersion))
                            {
                                dynamoVersions.Add(dynamoVersion);
                            }
                        }
                    }

                    // Return a new compatibility entry for Dynamo with the corresponding min, max, and specific versions (if any)
                    return new Greg.Responses.Compatibility
                    {
                        name = "Dynamo",
                        min = matchingMin,
                        max = matchingMax,
                        versions = dynamoVersions.Any() ? dynamoVersions : null
                    };
                }
            }

            // Return null if no matching host or Dynamo versions were found
            return null;
        }

        private VersionInformation GetLatestCompatibleVersion()
        {
            // Find the last compatible version
            var compatibleVersion = VersionDetails?.LastOrDefault(v => v.IsCompatible == true);

            // If no compatible version is found, return the latest version
            return compatibleVersion ?? VersionDetails?.LastOrDefault() ?? null;
        }
    }
}
