using System;
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
        /// <returns>List of KeyValuePair</returns>
        public static IDictionary<string, Tuple<int, int, int, int>> 
            FindDynamoInstallations(string additionalDynamoPath)
        {
            var installs = DynamoProducts.FindDynamoInstallations(additionalDynamoPath);
            return installs.Products.ToDictionary(p => p.InstallLocation, p => p.VersionInfo);
        }

        /// <summary>
        /// Finds all products installed on the system with given product name
        /// search pattern and file name search pattern. e.g. to find Dynamo
        /// installations, we can use Dynamo as product search pattern and
        /// DynamoCore.dll as file search pattern.
        /// </summary>
        /// <param name="productSearchPattern">search key for product</param>
        /// <param name="fileSearchPattern">search key for files</param>
        /// <returns>Dictionary of product install location and version info 
        /// of the file found in the installation based on file search pattern.
        /// </returns>
        public static IDictionary<string, Tuple<int, int, int, int>> 
            FindProductInstallations(string productSearchPattern, string fileSearchPattern)
        {
            var installs = new InstalledProducts();
            installs.LookUpAndInitProducts(new InstalledProductLookUp(productSearchPattern, fileSearchPattern));

            return installs.Products.ToDictionary(p => p.InstallLocation, p => p.VersionInfo);
        }
    }
}
