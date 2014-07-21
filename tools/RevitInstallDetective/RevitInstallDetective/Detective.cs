using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.RevitAddIns;

namespace RevitInstallDetective
{
    public static class Detective
    {
        /// <summary>
        /// Checks for an installation using the product type.  Any Revit 2013 install
        /// will be matched with "Revit2013", if the product is installed.  This method is 
        /// only valid for Revit 2013 forward.
        /// 
        /// Vasari Beta 3 can be searched for with "VasariBeta3"
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool InstallationExists(string version)
        {
            // Vasari Beta 3's product type is Revit2014, so we have to use the name
            return version.StartsWith("Vasari") ? NameExists(version) : ProductTypeExists(version);
        }

        #region Helpers

        private static IEnumerable<RevitProduct> AllInstalledProducts()
        {
            return RevitProductUtility.GetAllInstalledRevitProducts();
        }

        private static bool NameExists(string name)
        {
            return AllInstalledProducts().Any(x => x.Name.Replace(" ","") == name);
        }

        private static bool ProductTypeExists(string version)
        {
            return AllInstalledProducts().Any(x => 
                x.Version.ToString() == version && 
                x.Name.Replace(" ","") != "VasariBeta3"); // prevent false positive for Revit 2014 when Vasari Beta 3 is installed
        }

        #endregion

    }
}
