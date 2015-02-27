using System;
using System.Collections.Generic;

using Dynamo.Utilities;

using Greg.Responses;

namespace Dynamo.PackageManager
{
    public static class PackageUtilities
    {
        /// <summary>
        /// Obtain the packages from a list of packages that were created using a newer version
        /// of Dynamo than this one.
        /// </summary>
        public static IEnumerable<Tuple<PackageHeader, PackageVersion>> FilterFuturePackages(
            this IEnumerable<Tuple<PackageHeader, PackageVersion>> headerVersionPairs,
            Version currentAppVersion, int numberOfFieldsToCompare = 3)
        {
            foreach (var pair in headerVersionPairs)
            {
                var version = pair.Item2;
                var depAppVersion = VersionUtilities.PartialParse(version.engine_version, numberOfFieldsToCompare);

                if (depAppVersion > currentAppVersion)
                {
                    yield return pair;
                }
            }
        }
    }
}