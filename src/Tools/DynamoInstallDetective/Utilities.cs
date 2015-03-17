using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace DynamoInstallDetective
{
    /// <summary>
    /// Utility class for install detective
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Finds all unique Dynamo installations on the system
        /// </summary>
        /// <param name="additionalDynamoPath">Additional path for Dynamo binaries
        /// to be included in search</param>
        /// <returns>List of KeyValuePair of install location and version info 
        /// as Tuple. The returned list is sorted based on version info.</returns>
        public static IEnumerable FindDynamoInstallations(string additionalDynamoPath)
        {
            var installs = DynamoProducts.FindDynamoInstallations(additionalDynamoPath);
            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                        p.InstallLocation,
                        p.VersionInfo));
        }

        /// <summary>
        /// Finds all products installed on the system with given product name
        /// search pattern and file name search pattern. e.g. to find Dynamo
        /// installations, we can use Dynamo as product search pattern and
        /// DynamoCore.dll as file search pattern.
        /// </summary>
        /// <param name="productSearchPattern">search key for product</param>
        /// <param name="fileSearchPattern">search key for files</param>
        /// <returns>List of KeyValuePair of product install location and 
        /// version info as Tuple of the file found in the installation based 
        /// on file search pattern. The returned list is sorted based on version 
        /// info.</returns>
        public static IEnumerable FindProductInstallations(string productSearchPattern, string fileSearchPattern)
        {
            var installs = new InstalledProducts();
            installs.LookUpAndInitProducts(new InstalledProductLookUp(productSearchPattern, fileSearchPattern));

            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                        p.InstallLocation,
                        p.VersionInfo));
        }
    }
}
