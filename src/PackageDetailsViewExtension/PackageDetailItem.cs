using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.PythonServices;
using Greg.Responses;

namespace Dynamo.PackageDetails
{
    /// <summary>
    /// A simple utility class to allow an easier display of package version compatibility information
    /// </summary>
    public class FlattenedCompatibility
    {
        /// <summary>
        /// The Dynamo/Host name to be compatible with - "Dynamo", "Revit", "Civil3D" ..
        /// </summary>
        public string CompatibilityName { get; set; }
        /// <summary>
        /// The string representation of the numerical version of the package - "1.0.0"
        /// </summary>
        public string Versions { get; set; }
    }

    /// <summary>
    /// A wrapper class for a PackageVersion object, used in the PackageDetailsExtension.
    /// </summary>
    public class PackageDetailItem : NotificationObject
    {
        #region Private Fields

        private PackageVersion packageVersion;
        private string packageVersionNumber;
        private string hosts;
        private string pythonVersion;
        private string copyRightHolder;
        private string copyRightYear;
        private List<string> packages;
        private bool canInstall;
        private bool isEnabledForInstall;
        private string packageName;
        private string packageSize;
        private string created;
        private List<FlattenedCompatibility> versionInformation;
        private bool? isCompatible;
        private string releaseNotes;
        private bool isExpanded;

        private PackageLoader PackageLoader { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// A reference to the PackageVersion object, a response from the GregClient.
        /// </summary>
        public PackageVersion PackageVersion
        {
            get => packageVersion;
            set
            {
                packageVersion = value;
                RaisePropertyChanged(nameof(PackageVersion));
            }
        }

        /// <summary>
        /// The name of the package this PackageDetailItem represents a specific version of.
        /// </summary>
        public string PackageName
        {
            get => packageName;
            set
            {
                packageName = value; 
                RaisePropertyChanged(nameof(PackageName));
            }
        }

        /// <summary>
        /// The version number of this package, e.g. 0.0.0.1
        /// </summary>
        public string PackageVersionNumber
        {
            get => packageVersionNumber;
            set
            {
                packageVersionNumber = value;
                RaisePropertyChanged(nameof(PackageVersionNumber));
            }
        }

        /// <summary>
        /// The host software(s) this package relies upon, e.g. Revit, Civil3D
        /// </summary>
        public string Hosts
        {
            get => hosts;
            set
            {
                hosts = value;
                RaisePropertyChanged(nameof(Hosts));
            }
        }

        /// <summary>
        /// The Copyright holder
        /// </summary>
        public string CopyRightHolder
        {
            get => copyRightHolder;
            set
            {
                copyRightHolder = value;
                RaisePropertyChanged(nameof(CopyRightHolder));
            }
        }

        /// <summary>
        /// The Copyright Year
        /// </summary>
        public string CopyRightYear
        {
            get => copyRightYear;
            set
            {
                copyRightYear = value;
                RaisePropertyChanged(nameof(CopyRightYear));
            }
        }

        /// <summary>
        /// The version of Python referenced in this package, if any
        /// </summary>
        public string PythonVersion
        {
            get => pythonVersion;
            set
            {
                pythonVersion = value;
                RaisePropertyChanged(nameof(PythonVersion));
            }
        }

        /// <summary>
        /// The other packages contained within this package, if any.
        /// </summary>
        public List<string> Packages
        {
            get => packages;
            set
            {
                packages = value;
                RaisePropertyChanged(nameof(Packages));
            }
        }

        /// <summary>
        /// The size of the current package version.
        /// //TODO: Point this property to the package version size after it has been added to the db.
        /// </summary>
        public string PackageSize
        {
            get => packageSize;
            set
            {
                packageSize = value;
                RaisePropertyChanged(nameof(PackageSize));
            }
        }

        /// <summary>
        /// Returs true if package version is not already installed,
        /// false if already installed.
        /// </summary>
        public bool CanInstall
        {
            get => canInstall;
            set
            {
                canInstall = value;
                RaisePropertyChanged(nameof(CanInstall));
            }
        }

        /// <summary>
        /// Returs true if package is enabled for download and custom package paths are not disabled,
        /// False if custom package paths are disabled or package is deprecated, or package is already installed.
        /// </summary>
        public bool IsEnabledForInstall
        {
            get => isEnabledForInstall;
            set
            {
                isEnabledForInstall = value;
                RaisePropertyChanged(nameof(IsEnabledForInstall));
            }
        }

        /// <summary>
        /// The created date
        /// </summary>
        public string Created
        {
            get => created;
            set
            {
                created = value;
                RaisePropertyChanged(nameof(Created));
            }
        }

        /// <summary>
        /// A flattened collection of VersionInformation
        /// </summary>
        public List<FlattenedCompatibility> VersionInformation
        {
            get => versionInformation;
            set
            {
                versionInformation = value;
                RaisePropertyChanged(nameof(VersionInformation));
            }
        }

        /// <summary>
        /// If the current version is compatible, incompatible or with unknown compatibility
        /// </summary>
        public bool? IsCompatible
        {
            get => isCompatible;
            set
            {
                isCompatible = value;
                RaisePropertyChanged(nameof(IsCompatible));
            }
        }

        /// <summary>
        /// Returns the URL to the online package release notes
        /// </summary>
        public string ReleaseNotes
        {
            get => releaseNotes;
            set
            {
                releaseNotes = value;
                RaisePropertyChanged(nameof(ReleaseNotes));
            }
        }

        /// <summary>
        /// Controls the package detail item expanded state
        /// </summary>
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageVersion"></param>
        /// <param name="canInstall"></param>
        /// <param name="isEnabledForInstall">True, if package is not already downloaded, is not deprecated, and package loading is allowed.</param>
        public PackageDetailItem(string packageName, PackageVersion packageVersion, bool canInstall, bool isEnabledForInstall = true)
        {
            this.PackageName = packageName;
            this.PackageVersion = packageVersion;
            this.PackageVersionNumber = PackageVersion.version;
            this.CopyRightHolder = PackageVersion.copyright_holder;
            this.CopyRightYear = PackageVersion.copyright_year;
            this.CanInstall = canInstall;
            this.IsEnabledForInstall = isEnabledForInstall && canInstall;
            this.PackageSize = string.IsNullOrEmpty(PackageVersion.size) ? "--" : PackageVersion.size;
            this.Created = GetFormattedDate(PackageVersion.created);
            this.VersionInformation = GetFlattenedCompatibilityInformation(packageVersion.compatibility_matrix);
            this.ReleaseNotes = PackageVersion.release_notes_url;

            // To avoid displaying package self-dependencies.
            // For instance, avoiding Clockwork showing that it depends on Clockwork.
            this.Packages = PackageVersion.full_dependency_ids
                .Select(x => x.name)
                .Where(x => x != PackageName)
                .ToList();

            DetectDependencies();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="versionDetails"></param>
        /// <param name="packageName"></param>
        /// <param name="packageVersion"></param>
        /// <param name="canInstall"></param>
        /// <param name="isEnabledForInstall">True, if package is not already downloaded, is not deprecated, and package loading is allowed.</param>
        public PackageDetailItem(List<VersionInformation> versionDetails, string packageName, PackageVersion packageVersion, bool canInstall, bool isEnabledForInstall = true)
        {
            this.PackageName = packageName;
            this.PackageVersion = packageVersion;
            this.PackageVersionNumber = PackageVersion.version;
            this.CopyRightHolder = PackageVersion.copyright_holder;
            this.CopyRightYear = PackageVersion.copyright_year;
            this.CanInstall = canInstall;
            this.IsEnabledForInstall = isEnabledForInstall && canInstall;
            this.PackageSize = string.IsNullOrEmpty(PackageVersion.size) ? "--" : PackageVersion.size;
            this.Created = GetFormattedDate(PackageVersion.created);
            this.VersionInformation = GetFlattenedCompatibilityInformation(packageVersion.compatibility_matrix);
            this.IsCompatible = PackageManager.VersionInformation.GetVersionCompatibility(versionDetails, PackageVersionNumber);
            this.ReleaseNotes = PackageVersion.release_notes_url;

            // To avoid displaying package self-dependencies.
            // For instance, avoiding Clockwork showing that it depends on Clockwork.
            this.Packages = PackageVersion.full_dependency_ids
                .Select(x => x.name)
                .Where(x => x != PackageName)
                .ToList();

            DetectDependencies();
        }

        /// <summary>
        /// Flattens the list of compatibility information from a collection of version infos.
        /// Each compatibility entry is represented by its name and associated versions.
        /// </summary>
        /// <param name="versionDetails">A list of version compatibility information. Each entry contains a compatibility name, version range, and version list.</param>
        /// <returns>A list of <see cref="FlattenedCompatibility"/> objects, each representing a flattened compatibility with its name and formatted versions.</returns>
        private List<FlattenedCompatibility> GetFlattenedCompatibilityInformation(List<Greg.Responses.Compatibility> versionDetails)
        {
            var flattenedCompatibilities = new List<FlattenedCompatibility>();
            try
            {   
                foreach (var versionInformation in versionDetails)
                {
                    // Add each compatibility dynamically based on the name
                    flattenedCompatibilities.Add(new FlattenedCompatibility
                    {
                        CompatibilityName = CapitalizeFirstLetter(versionInformation.name),
                        Versions = FormatVersionString(versionInformation)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }   

            return flattenedCompatibilities;
        }

        /// <summary>
        /// Formats the version information from a <see cref="Greg.Responses.Compatibility"/> object into a single string.
        /// Combines the minimum and maximum versions with the version list if available.
        /// </summary>
        /// <param name="compatibility">The <see cref="Greg.Responses.Compatibility"/> object containing versioning information.</param>
        /// <returns>A formatted string representing the version range and individual versions, or a comma-delimited string of versions if no range is provided.</returns>
        private string FormatVersionString(Greg.Responses.Compatibility compatibility)
        {
            var hasVersions = compatibility.versions != null && compatibility.versions.Any();

            if (!string.IsNullOrEmpty(compatibility.min) && !string.IsNullOrEmpty(compatibility.max) && hasVersions)
            {
                return $"{compatibility.min} - {compatibility.max}, {string.Join(", ", compatibility.versions)}";
            }
            else if (!string.IsNullOrEmpty(compatibility.min) && !string.IsNullOrEmpty(compatibility.max))
            {
                return $"{compatibility.min} - {compatibility.max}";
            }
            else if (hasVersions)
            {
                return string.Join(", ", compatibility.versions);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Try parse a date string in a preferred format
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        private string GetFormattedDate(string dateString)
        {
            if (String.IsNullOrEmpty(dateString)) return string.Empty;

            DateTime parsedDate;

            // Attempt to parse using a preferred format (ISO 8601, for example)
            string[] preferredFormats = { "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ" };

            foreach (var format in preferredFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
                {
                    return parsedDate.ToString("dd MMM yyyy");
                }
            }

            // Fallback to the more flexible DateTime parsing
            if (DateTime.TryParse(dateString, out parsedDate))
            {
                return parsedDate.ToString("dd MMM yyyy");
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads the GregResponse collection of dependency information and sets values
        /// for PythonVersion and Hosts respectively.
        /// </summary>
        private void DetectDependencies()
        {
            List<string> pythonEngineVersions = new List<string>();
            List<string> hostDependencies = new List<string>();

            if (PackageVersion?.host_dependencies != null)
            {
                foreach (string stringValue in PackageVersion.host_dependencies)
                {
                    if (stringValue == PythonEngineManager.IronPython2EngineName ||
                        stringValue == PythonEngineManager.CPython3EngineName)
                    {
                        pythonEngineVersions.Add(stringValue);
                    }
                    else
                    {
                        hostDependencies.Add(stringValue);
                    }
                }
            }

            PythonVersion = pythonEngineVersions.Count > 0 ? string.Join(", ", pythonEngineVersions) : Dynamo.Properties.Resources.NoneString;
            Hosts = hostDependencies.Count > 0 ? string.Join(", ", hostDependencies) : Dynamo.Properties.Resources.NoneString;
        }

        private static string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }
}
