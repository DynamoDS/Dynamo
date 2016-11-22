using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACGClientForCEF.Utility
{
    public static class PackageUtilities
    {
        public static bool IsNewerVersion(string version, string compareTo)
        {

            var splitVersion =
                version.Split('.').Where(x => x.ToCharArray().All(Char.IsNumber)).Select(int.Parse).ToList();
            var splitCompareTo =
                compareTo.Split('.').Where(x => x.ToCharArray().All(Char.IsNumber)).Select(int.Parse).ToList();

            // return false if the versions are malformed
            if (splitVersion.Count != 3 || splitCompareTo.Count != 3)
            {
                return false;
            }

            var incr = false;

            for (var ind = 0; ind < splitVersion.Count; ind++)
            {
                if (splitVersion[ind] < splitCompareTo[ind])
                {
                    incr = true;
                    break;
                }

                if (splitVersion[ind] > splitCompareTo[ind]) break;
            }

            return incr;
        }
    }
}
