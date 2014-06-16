using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.RevitAddIns;

namespace DynamoAddinGenerator
{
    class Program
    {
        private const string addinName = "Dynamo.addin";
        private const string versionSelectorName = "DynamoVersionSelector.addin";

        private static string dynamo_063 = @"C:\Autodesk\Dynamo";
        private static string dynamo_071_x86 = Path.Combine(Environment.SpecialFolder.ProgramFilesX86.ToString(),"Dynamo071");
        private static string dynamo_071_bad = Path.Combine(Environment.SpecialFolder.ProgramFiles.ToString(),"Dynamo071");
        private static string dynamo_post_071 = Path.Combine(Environment.SpecialFolder.ProgramFiles.ToString(), "Dynamo");

        static void Main(string[] args)
        {
            var installedProds = GetInstalledProducts();

            var prods = GetValidProducts(installedProds).ToList();

            if (!prods.Any())
            {
                Console.WriteLine("There were no Revit products found.");
                return;
            }

            DeleteAddins();

            

            var dynamos = GetInstalledDynamos();

            // Search each addin path for an existing Dynamo.addin
            // These are from installs without multiple versions
            GenerateAddins(prods, dynamos);
        }

        private static IEnumerable<IRevitProduct> GetInstalledProducts()
        {
            var installedProds = RevitProductUtility.GetAllInstalledRevitProducts()
                .ToList()
                .Select(p => new DynamoRevitProduct(p));

            return installedProds;
        }

        private static bool HasDynamo063Install()
        {
            if (Directory.Exists(dynamo_063))
            {
                return true;
            }
            return false;
        }

        private static bool HasDynamo071BadInstall()
        {
            if (Directory.Exists(dynamo_071_bad))
            {
                return true;
            }
            return false;
        }

        private static bool HasDynamo071InstallInx86()
        {
            if (Directory.Exists(dynamo_071_x86))
            {
                return true;
            }
            return false;
        }

        private static bool HasDynamo071Install()
        {
            if (Directory.Exists(dynamo_post_071))
            {
                return true;
            }
            return false;
        }

        private static IEnumerable<IDynamoInstall> GetInstalledDynamos()
        {
            var dynamos = new List<IDynamoInstall>();

            if (HasDynamo063Install())
            {
                
            }

            if (HasDynamo071BadInstall())
            {
                
            }

            if (HasDynamo071InstallInx86())
            {
                
            }

            if (HasDynamo071Install())
            {
                
            }

            return dynamos;
        }

        internal static IEnumerable<IRevitProduct> GetValidProducts(IEnumerable<IRevitProduct> products)
        {
            var validProds = new List<IRevitProduct>();

            foreach (var prod in products)
            {
                if (prod.VersionString == RevitVersion.Revit2011.ToString() ||
                    prod.VersionString == RevitVersion.Revit2012.ToString() ||
                    prod.VersionString == RevitVersion.Revit2013.ToString())
                {
                    continue;
                }

                validProds.Add(prod);
            }

            return validProds;
        }

        internal static void BackupOldAddins(IEnumerable<IRevitProduct> products)
        {
            foreach (var product in products)
            {
                var oldAddinPath = Path.Combine(product.AddinPath, "Dynamo.addin");
                var newAddinPath = Path.Combine(product.AddinPath, "Dynamo.OLD");

                if (File.Exists(oldAddinPath))
                {
                    File.Move(oldAddinPath,newAddinPath);
                }
            }
        }

        /// <summary>
        /// Deletes all existing Dynamo addins.
        /// </summary>
        internal static void DeleteAddins()
        {
            
        }

        /// <summary>
        /// Generates new Dynamo addins.
        /// </summary>
        /// <param name="products">A collection of revit installs.</param>
        /// <param name="dynamos">A collection of dynamo installs.</param>
        internal static void GenerateAddins(IEnumerable<IRevitProduct> products, IEnumerable<IDynamoInstall> dynamos)
        {
            // If there is only one version of revit installed, we will
            // create a Dynamo.addin, and return.
            if (products.Count() == 1)
            {
                // Generate a dynamo.addin and return
                GenerateDynamoAddin(products.FirstOrDefault());
                return;
            }

            // If there are multiple versions of Revit installed, we will
            // create a DynamoVersionSelector.addin.
            foreach (var prod in products)
            {
                GenerateVersionSelectorAddin(prod);
            }
        }

        /// <summary>
        /// Generate a single-version Dynamo addin.
        /// </summary>
        /// <param name="product"></param>
        internal static void GenerateDynamoAddin(IRevitProduct product)
        {
            
            
        }

        /// <summary>
        /// Generate a multiple-version Dynamo addin.
        /// </summary>
        /// <param name="product"></param>
        internal static void GenerateVersionSelectorAddin(IRevitProduct product)
        {

            
        }
    }

    public class DynamoRevitProduct : IRevitProduct
    {
        public string ProductName { get; set; }
        public string AddinPath { get; set; }
        public string InstallLocation { get; set; }
        public string VersionString { get; set; }
        public string CurrentDynamoAddinPath { get; set; }

        public DynamoRevitProduct(RevitProduct product)
        {
            ProductName = product.Name;
            AddinPath = product.AllUsersAddInFolder;
            InstallLocation = product.InstallLocation;
            VersionString = product.Version.ToString();

            var oldAddinPath = Path.Combine(AddinPath, "Dynamo.addin");
            var newAddinPath = Path.Combine(AddinPath, "DynamoVersionSelector.addin");

            if (File.Exists(oldAddinPath))
            {
                CurrentDynamoAddinPath = oldAddinPath;
            }
            else if (File.Exists(newAddinPath))
            {
                CurrentDynamoAddinPath = newAddinPath;
            }
        }
    }

    public class DynamoInstall:IDynamoInstall
    {
        public string Path { get; set; }

        public DynamoInstall(string path)
        {
            Path = path;
        }
    }

    public interface IRevitProduct
    {
        string ProductName { get; set; }
        string AddinPath { get; set; }
        string InstallLocation { get; set; }
        string VersionString { get; set; }
        string CurrentDynamoAddinPath { get; set; }
    }

    public interface IDynamoInstall
    {
        string Path { get; set; }
    }
}
