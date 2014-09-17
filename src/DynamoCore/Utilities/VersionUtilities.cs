using System;
using System.Linq;

namespace Dynamo.Utilities
{
    internal static class VersionUtilities
    {
        /// <summary>
        /// Parse the first n fields of a version string.  Delegates to
        /// Version.Parse.
        /// </summary>
        internal static Version PartialParse(string versionString, int numberOfFields = 3)
        {
            var splitVersion = versionString.Split('.');
            if (splitVersion.Length < numberOfFields)
                throw new FormatException("You specified too many fields for the given string.");

            var rejoinedVersion = string.Join(".", splitVersion.Take(numberOfFields));

            return Version.Parse(rejoinedVersion);
        }
    }

}
