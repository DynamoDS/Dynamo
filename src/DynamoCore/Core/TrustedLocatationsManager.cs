using Dynamo.Configuration;
using DynamoUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Core
{
    /// <summary>
    /// Manages trusted locations for Dynamo
    /// </summary>
    public class TrustedLocatationsManager
    {
        /// <summary>
        /// Use Lazy&lt;PythonEngineManager&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<TrustedLocatationsManager>
            lazy =
            new Lazy<TrustedLocatationsManager>
            (() => new TrustedLocatationsManager());

        private PreferenceSettings settings;

        /// <summary>
        /// Dynamo's trusted locations.
        /// </summary>
        public List<string> TrustedLocations
        {
            get => settings.TrustedLocations;
            internal set
            {
                settings.TrustedLocations = value;
            }
        }

        internal void Initialize(PreferenceSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #region Public members
        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static TrustedLocatationsManager Instance
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Checkes whether the input argument (path) is among Dynamo's trusted locations
        /// Only directories are supported (if a file path is used, its root directry will be checked).
        /// </summary>
        /// <param name="path">An absolute path to a folder or file on disk</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Input argument is not an absolute path.</exception>
        /// <exception cref="ArgumentException">Path directory does not exist</exception>
        /// <exception cref="System.Security.SecurityException">Dynamo does not have the required permissions.</exception>
        public bool IsTrustedFolder(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException($"The input argument {path} must be an absolute path");
            }

            if (!PathHelper.HasReadPermissionOnDir(path))
            {
                throw new System.Security.SecurityException($"Dynamo does not have the required permissions for the path: {path}");
            }

            string location = Path.GetFullPath(path);
            if (string.IsNullOrEmpty(Path.GetFileName(location)))
            {
                // File path
                location = Path.GetPathRoot(location);
            }

            if (!Directory.Exists(location))
            {
                throw new ArgumentException($"Folder path {location} does not exist");
            }

            // All subdirectories are considered trusted if the parent directory is trusted.
            var trustedLoc = TrustedLocations.FirstOrDefault(x => location.StartsWith(x));
            return !string.IsNullOrEmpty(trustedLoc);
        }
        #endregion
    }
}
