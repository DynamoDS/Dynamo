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
                throw new FormatException(/*NXLT*/"You specified too many fields for the given string.");

            var rejoinedVersion = string.Join(/*NXLT*/".", splitVersion.Take(numberOfFields));

            return Version.Parse(rejoinedVersion);
        }
    }

}
