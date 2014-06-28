using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.RevitAddIns;

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
        /// Find a Dynamo install by checking for the existence of one of the
        /// install directories, and ensuring that it contains DynamoCore.
        /// </summary>
        /// <returns></returns>
        public static List<IDynamoInstall> FindDynamoInstalls(string debugPath = null)
        {
            var installs = new List<IDynamoInstall>();

            if (DynamoExistsAtPath(DynamoVersions.dynamo_063))
            {
                installs.Add(new DynamoInstall(DynamoVersions.dynamo_063));
            }

            if (DynamoExistsAtPath(DynamoVersions.dynamo_071_x86))
            {
                installs.Add(new DynamoInstall(DynamoVersions.dynamo_071_x86));
            }

            if (DynamoExistsAtPath(DynamoVersions.dynamo_071_x64))
            {
                installs.Add(new DynamoInstall(DynamoVersions.dynamo_071_x64));
            }

            if (DynamoExistsAtPath(DynamoVersions.dynamo_07x))
            {
                installs.Add(new DynamoInstall(DynamoVersions.dynamo_07x));
            }

            if (!string.IsNullOrEmpty(debugPath))
            {
                installs.Add(new DynamoInstall(debugPath));
            }

            return installs;
        }

        private static bool DynamoExistsAtPath(string basePath)
        {
            return Directory.Exists(basePath) && Directory.GetFiles(basePath,"*DynamoCore.dll").Any();
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
        /// <param name="latestDynamoFolder">The newest Dynamo version installed on the machine.</param>
        public DynamoAddinData(IRevitProduct product, IDynamoInstall latestDynamoInstall)
        {
            var subfolder = "";
            if (latestDynamoInstall.Folder == DynamoVersions.dynamo_063)
            {
                ClassName = "Dynamo.Applications.DynamoRevitApp";
                AssemblyPath = Path.Combine(latestDynamoInstall.Folder, "DynamoRevit.dll");
            }
            else
            {
                ClassName = "Dynamo.Applications.VersionLoader";
                if (product.VersionString == "Revit2014")
                {
                    subfolder = "Revit_2014";
                }
                else if (product.VersionString == "Revit2015")
                {
                    subfolder = "Revit_2015";
                }
                AssemblyPath = Path.Combine(latestDynamoInstall.Folder, subfolder, "DynamoRevitVersionSelector.dll");
            }

            RevitSubfolder = subfolder;
            AddinPath = Path.Combine(product.AddinsFolder, "Dynamo.addin");
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
