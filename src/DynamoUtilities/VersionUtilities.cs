using System;
using System.Linq;

namespace Dynamo.Utilities
{
    public static class VersionUtilities
    {
        /// <summary>
        /// Parse the first n fields of a version string.  Delegates to
        /// Version.Parse.
        /// </summary>
        public static Version PartialParse(string versionString, int numberOfFields = 3)
        {
            var splitVersion = versionString.Split('.');
            if (splitVersion.Length < numberOfFields)
                throw new FormatException("You specified too many fields for the given string.");

            var rejoinedVersion = string.Join(".", splitVersion.Take(numberOfFields));

            return Version.Parse(rejoinedVersion);
        }

        /// <summary>
        /// A utility method accepting any string input and trying to return a valid Version from it
        /// If the initial input is not valid, returns null
        /// </summary>
        /// <param name="version">The string representation of the version to be parsed.</param>
        /// <returns></returns>
        public static Version Parse(string version)
        {
            // If the version string is null or empty, return null
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            // Split the version by periods
            var versionParts = version.Split('.');

            // Ensure each part is numeric; if not, return null
            foreach (var part in versionParts)
            {
                if (!int.TryParse(part, out _))
                {
                    return null;
                }
            }

            // Try parsing directly; if successful, return the parsed version
            if (Version.TryParse(version, out Version parsedVersion))
            {
                return NormalizeVersion(parsedVersion);
            }

            // If parsing failed, pad the version string
            while (versionParts.Length < 3)
            {
                version += ".0";
                versionParts = version.Split('.');
            }

            // Now it should be safe to parse - this catches any other incompatible Versions
            // Including '1.2.3.4.5'
            return Version.TryParse(version, out parsedVersion) ? parsedVersion : null;
        }

        /// <summary>
        /// The maximum version we check against when substituting a wildcard
        /// </summary>
        private const string WILDCARD_MAX_VERSION = "2147483647";

        /// <summary>
        /// Parse the first n fields of a version string. Delegates to
        /// Version.Parse.
        /// </summary>
        public static Version WildCardParse(string version)
        {
            // If the version string is null or empty, return null
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            // Check if the version string ends with a wildcard
            if (!version.EndsWith(".*"))
            {
                return null;
            }

            var splitVersion = version.Split('.');

            // Check that there are no more than 3 version items specified
            if (splitVersion.Length > 3)
            {
                return null;
            }

            // Ensure the first part is a valid number
            if (!int.TryParse(splitVersion[0], out _))
            {
                return null;
            }

            string newVersion;
            if (splitVersion.Length == 2)
            {
                // Major and wildcard
                newVersion = AppendWildcardVersion(splitVersion[0], WILDCARD_MAX_VERSION, "0");
            }
            else if (splitVersion.Length == 3)
            {
                // Major, minor, and wildcard
                if (!int.TryParse(splitVersion[1], out _))
                {
                    return null;
                }
                newVersion = AppendWildcardVersion(splitVersion[0], splitVersion[1], WILDCARD_MAX_VERSION);
            }
            else
            {
                return null;
            }

            return Version.TryParse(newVersion, out Version parsedVersion) ? parsedVersion : null;
        }

        private static string AppendWildcardVersion(string major, string minor = WILDCARD_MAX_VERSION, string patch = "0")
        {
            return $"{major}.{minor}.{patch}";
        }

        /// <summary>
        /// Helper method to normalize versions to a 3-part format
        /// </summary>
        /// <param name="version">A potentially partial version</param>
        /// <returns></returns>
        internal static Version NormalizeVersion(Version version)
        {
            return new Version(
                version.Major,
                version.Minor == -1 ? 0 : version.Minor,
                version.Build == -1 ? 0 : version.Build
            );
        }
    }
}
