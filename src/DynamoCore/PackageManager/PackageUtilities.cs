using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Greg.Responses;

namespace Dynamo.PackageManager
{
    public static class PackageUtilities
    {

        public static IEnumerable<Tuple<PackageHeader, string>> FilterFuturePackages(
            this IEnumerable<Tuple<PackageHeader, string>> headerVersionPairs,
            string currentVersion)
        {
            foreach (var pair in headerVersionPairs)
            {
                var header = pair.Item1;
                var vname = pair.Item2;

                var depVersion = header.versions.First(x => x.version == vname);

                if (depVersion.engine_version.IsGreaterVersionThan(currentVersion))
                {
                    yield return pair;
                }
            }
        }

        public static bool IsGreaterVersionThan(this string version, string versionToCompare)
        {
            var splitVersion = version.Split('.').Select(int.Parse).Take(3).ToList();
            var splitVersionToCompare = versionToCompare.Split('.').Select(int.Parse).Take(3).ToList();

            for (var i = 0; i < 3; i++)
            {
                if (splitVersion[i] > splitVersionToCompare[i]) return true;
                if (splitVersion[i] < splitVersionToCompare[i]) return false;
            }

            return false;
        }

    }
}
