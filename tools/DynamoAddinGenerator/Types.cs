using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.RevitAddIns;
using DynamoInstallDetective;

namespace DynamoAddinGenerator
{
    public static class DynamoVersions
    {
        public static string dynamo_063 = @"C:\Autodesk\Dynamo\Core";
        public static string dynamo_071_x86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Dynamo071");
        public static string dynamo_071_x64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dynamo071");
        public static string dynamo_07x = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dynamo 0.7");
    }

    /// <summary>
    /// A container with information about a revit product.
    /// </summary>
    public class DynamoRevitProduct : IRevitProduct
    {
        public string ProductName { get; set; }
        public string AddinsFolder { get; set; }
        public string InstallLocation { get; set; }
        public string VersionString { get; set; }

        public DynamoRevitProduct(RevitProduct product)
        {
            ProductName = product.Name;
            AddinsFolder = product.AllUsersAddInFolder;
            InstallLocation = product.InstallLocation;
            VersionString = product.Version.ToString();
        }
    }

    /// <summary>
    /// A container with information about a Dynamo install.
    /// </summary>
    public class DynamoInstall : IDynamoInstall
    {
        public string Folder { get; set; }

        public DynamoInstall(string folder)
        {
            Folder = folder;
        }

        public static bool PathEquals(string path1, string path2)
        {
            return string.Equals(Path.GetFullPath(path1), Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// A collection of dynamo installs.
    /// </summary>
    public class DynamoInstallCollection : IDynamoInstallCollection
    {
        /// <summary>
        /// A list of dynamo installs in ascending version order.
        /// </summary>
        public IEnumerable<IDynamoInstall> Installs { get; set; }

        public DynamoInstallCollection(IEnumerable<IDynamoInstall> installs)
        {
            Installs = installs;
        }

        /// <summary>
        /// Finds all Dynamo installations on the system by looking at registry
        /// and install directories, and ensuring that it contains DynamoCore.
        /// </summary>
        /// <returns>List of IDynamoInstall</returns>
        public static IEnumerable<IDynamoInstall> FindDynamoInstalls(string debugPath)
        {
            var dynamos = DynamoProducts.FindDynamoInstallations(debugPath);

            foreach (IInstalledProduct product in dynamos.Products)
            {
                string[] files = Directory.GetFiles(product.InstallLocation, "DynamoRevit*.dll");
                string[] dirs = Directory.GetDirectories(product.InstallLocation, "Revit*");
                if (dirs.Any() || files.Any()) yield return new DynamoInstall(product.InstallLocation);
            }
        }

        public IDynamoInstall GetLatest()
        {
            return Installs.LastOrDefault();
        }
    }

    /// <summary>
    /// A container for data for creating an addin.
    /// </summary>
    public class DynamoAddinData : IDynamoAddinData
    {
        public string AssemblyPath { get; set; }
        public string AddinPath { get; set; }
        public string RevitSubfolder { get; set; }
        public string ClassName { get; set; }
        public string Id
        {
            get { return "8D83C886-B739-4ACD-A9DB-1BC78F315B2B"; }
        }

        /// <summary>
        /// DynamoAddinData constructor.
        /// </summary>
        /// <param name="product">A revit product to target for addin creation.</param>
        /// <param name="latestDynamoInstall">The newest Dynamo version installed on the machine.</param>
        internal DynamoAddinData(IRevitProduct product, IDynamoInstall latestDynamoInstall)
        {
            //Convert Revit2014 to Revit_2014
            var subfolder = product.VersionString.Insert(5, "_");

            //Pre 0.7.x release
            if (DynamoInstall.PathEquals(latestDynamoInstall.Folder,DynamoVersions.dynamo_063))
            {
                ClassName = "Dynamo.Applications.DynamoRevitApp";
                AssemblyPath = Path.Combine(latestDynamoInstall.Folder, "DynamoRevit.dll");
            }
            else
            {
                ClassName = "Dynamo.Applications.VersionLoader";
                const string assemblyName = "DynamoRevitVersionSelector.dll";

                AssemblyPath = Path.Combine(latestDynamoInstall.Folder, subfolder, assemblyName);
            }

            RevitSubfolder = subfolder;
            AddinPath = Path.Combine(product.AddinsFolder, "Dynamo.addin");
        }

        /// <summary>
        /// Creates DynamoAddinData to generate addin for given Revit product
        /// based on latest Dynamo installed on the system.
        /// </summary>
        /// <param name="revit">Revit Product for which addin to be generated </param>
        /// <param name="dynamos">Dynamo products installed on the system</param>
        /// <param name="dynamoUninstallPath">dynamo path being uninstalled, can be 
        /// null or empty string</param>
        /// <returns>DynamoAddinData</returns>
        public static DynamoAddinData Create(IRevitProduct revit, DynamoProducts dynamos, string dynamoUninstallPath)
        {
            //Iterate in reverse order to find the first dynamo that is supported for
            //this revit product
            var products = dynamos.Products.Reverse();
            foreach (var p in products)
            {
                //If the current product is being uninstalled, don't generate addin data
                if (DynamoInstall.PathEquals(p.InstallLocation, dynamoUninstallPath))
                    continue;

                var path = Path.Combine(p.InstallLocation, "DynamoRevit.dll");
                //Should be 0.6.3 only supported for Revit2014
                if(File.Exists(path) && revit.VersionString == "Revit2014") 
                    return new DynamoAddinData(revit, new DynamoInstall(p.InstallLocation));

                var subfolder = revit.VersionString.Insert(5, "_");
                path = Path.Combine(p.InstallLocation, subfolder, "DynamoRevitVersionSelector.dll");
                if (File.Exists(path))
                    return new DynamoAddinData(revit, new DynamoInstall(p.InstallLocation));
            }

            return null;
        }
    }

    /// <summary>
    /// A collection of Revit products.
    /// </summary>
    public class RevitProductCollection : IRevitProductCollection
    {
        public List<IRevitProduct> Products { get; set; }

        public RevitProductCollection(IEnumerable<IRevitProduct> products)
        {
            Products = new List<IRevitProduct>();

            foreach (var prod in products)
            {
                if (prod.VersionString == RevitVersion.Revit2011.ToString() ||
                    prod.VersionString == RevitVersion.Revit2012.ToString() ||
                    prod.VersionString == RevitVersion.Revit2013.ToString())
                {
                    continue;
                }

                Products.Add(prod);
            }
        }
    }

    public interface IRevitProductCollection
    {
        List<IRevitProduct> Products { get; }
    }

    public interface IRevitProduct
    {
        string ProductName { get; set; }
        string AddinsFolder { get; set; }
        string InstallLocation { get; set; }
        string VersionString { get; set; }
    }

    public interface IDynamoInstallCollection
    {
        IEnumerable<IDynamoInstall> Installs { get; set; }
        IDynamoInstall GetLatest();
    }

    public interface IDynamoInstall
    {
        string Folder { get; set; }
    }

    public interface IDynamoAddinData
    {
        string AssemblyPath { get; set; }
        string AddinPath { get; set; }
        string RevitSubfolder { get; set; }
        string Id { get; }
        string ClassName { get; set; }
    }
}
