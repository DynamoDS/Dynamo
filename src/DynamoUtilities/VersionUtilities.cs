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
                return parsedVersion;
            }

            // If parsing failed, pad the version string
            while (versionParts.Length < 3)
            {
                version += ".0";
                versionParts = version.Split('.');
            }

            // Now it should be safe to parse
            return Version.Parse(version);
        }

        /// <summary>
        /// Parse the first n fields of a version string.  Delegates to
        /// Version.Parse.
        /// </summary>
        public static Version WildCardParse(string version)
        {
            // If the version string is null or empty, return null
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            // Check if the version string ends with a wild card.
            if (!version.EndsWith(".*"))
            {
                return null;
            }

            var splitVersion = version.Split('.');

            //CHeck that there are not more then 3 version items specified
            if (splitVersion.Length > 3)
            {
                return null;
            }

            // Check that the first number is a valid number
            if (!int.TryParse(splitVersion[0], out _))
            {
                return null;
            }

            if (splitVersion.Length == 2)
            {
                var newVersion = splitVersion[0] + ".1000.0";
                if (Version.TryParse(newVersion, out Version parsedVersion))
                {
                    return parsedVersion;
                }

                return null;
            }

            if(splitVersion.Length == 3)
            {
                if (!int.TryParse(splitVersion[1], out _))
                {
                    return null;
                }

                var newVersion = splitVersion[0] + "." + splitVersion[1] + ".1000";
                if (Version.TryParse(newVersion, out Version parsedVersion))
                {
                    return parsedVersion;
                }

                return null;
            }

            return null;
        }
    }
}
