using Dynamo.Configuration;
using DynamoUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dynamo.Core
{
    /// <summary>
    /// Manages trusted locations for Dynamo
    /// </summary>
    public class TrustedLocationsManager
    {
        /// <summary>
        /// Use Lazy&lt;TrustedLocationsManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<TrustedLocationsManager>
            lazy =
            new Lazy<TrustedLocationsManager>
            (() => new TrustedLocationsManager());

        private PreferenceSettings settings;

        /// <summary>
        /// Dynamo's trusted locations.
        /// Dynamo will load .dyn files from these locations without asking the user for consent.
        /// </summary>
        private List<string> trustedLocations;

        /// <summary>
        /// An read only list of paths that represents Dynamo's trusted locations
        /// </summary>
        public IReadOnlyList<string> TrustedLocations => trustedLocations;

        internal void Initialize(PreferenceSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            trustedLocations = settings.TrustedLocations?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Add a path to the Dynamo's trusted locations
        /// </summary>
        /// <param name="path">The path to be added as a trusted location</param>
        /// <returns></returns>
        internal bool AddTrustedLocation(string path)
        {
            try
            {
                string location = ValidateLocation(path);
                var existing = TrustedLocations.FirstOrDefault(x => x.Equals(location));
                if (settings != null && string.IsNullOrEmpty(existing))
                {
                    trustedLocations.Add(location);
                    settings.SetTrustedLocations(trustedLocations);
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Remove a path from the Dynamo's trusted locations
        /// </summary>
        /// <param name="path">The path to be removed from the trusted locations</param>
        /// <returns>The true if the path was removed and false otherwise</returns>
        internal bool RemoveTrustedLocation(string path)
        {
            string location = ValidateLocation(path);
            if (settings != null && trustedLocations.RemoveAll(x => x.Equals(location)) >= 0)
            {
                settings.SetTrustedLocations(trustedLocations);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Overrides the entire trusted locations list with the input argument
        /// </summary>
        /// <param name="paths"></param>
        internal void SetTrustedLocations(IEnumerable<string> paths)
        {
            trustedLocations.Clear();
            foreach (var item in paths)
            {
                AddTrustedLocation(item);
            }
            settings.SetTrustedLocations(trustedLocations);
        }

        /// <summary>
        /// Checks is the path is considered valid for Dynamo's trusted locations.
        /// An exception is thrown if the path is considered invalid.
        /// A path is considered valid if the following conditions are true:
        /// 1. Path is not null and not empty.
        /// 2. Path is an absolute path (not relative).
        /// 4. Path has valid characters.
        /// 5. Path exists and points to a folder.
        /// 6. Dynamo has read permissions to access the path.
        /// </summary>
        /// <param name="directoryPath">The directory path that needs to be validated</param>
        /// <returns>A normalized and validated path</returns>
        /// <exception cref="ArgumentException">Input argument is null or empty.</exception>
        /// <exception cref="ArgumentException">Input argument is not an absolute path.</exception>
        /// <exception cref="ArgumentException">Path directory does not exist</exception>
        /// <exception cref="System.Security.SecurityException">Dynamo does not have the required permissions.</exception>
        internal string ValidateLocation(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"The input argument is null or empty.");
            }

            if (!Path.GetFullPath(directoryPath).Equals(directoryPath))
            {
                throw new ArgumentException($"The input path {directoryPath} must be an absolute path");
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new FileNotFoundException($"The input path: {directoryPath} does not exist or is not a folder");
            }

            if (!PathHelper.HasReadPermissionOnDir(directoryPath))
            {
                throw new System.Security.SecurityException($"Dynamo does not have the required permissions for the path: {directoryPath}");
            }

            return directoryPath;
        }

        #region Public members
        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static TrustedLocationsManager Instance
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Checkes whether the input argument (path) is among Dynamo's trusted locations
        /// Only directories are supported (if a file path is used, its root directry will be checked).
        /// </summary>
        /// <param name="path">An absolute path to a folder or file on disk</param>
        /// <returns>True if the path is a trusted location, false otherwise</returns>
        public bool IsTrustedLocation(string path)
        {
            string location = ValidateLocation(path);

            // All subdirectories are considered trusted if the parent directory is trusted.
            var trustedLoc = TrustedLocations.FirstOrDefault(x => location.StartsWith(x));
            return !string.IsNullOrEmpty(trustedLoc);
        }

        /// <summary>
        /// If true, trust warnings for opening .dyn files from untrusted locations will not be shown.
        /// </summary>
        public bool TrustWarningsDisabled => settings.DisableTrustWarnings;
        
        #endregion
    }
}
