using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Win32;

namespace DynamoInstallDetective
{
    /// <summary>
    /// Defines an installed product
    /// </summary>
    public interface IInstalledProduct : IComparable
    {
        /// <summary>
        /// Name of the Installed Product
        /// </summary>
        string ProductName { get; set; }

        /// <summary>
        /// Location of product installation
        /// </summary>
        string InstallLocation { get; set; }

        /// <summary>
        /// Product version string 
        /// </summary>
        string VersionString { get; set; }

        /// <summary>
        /// Version info for major, minor, build no. and revision number
        /// </summary>
        Tuple<int, int, int, int> VersionInfo { get; }
    }

    /// <summary>
    /// Implements search algorithm for a installed product
    /// </summary>
    public interface IProductLookUp
    {
        /// <summary>
        /// Gets installed product from the installation path
        /// </summary>
        /// <param name="path">Installation path</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromInstallPath(string path);

        /// <summary>
        /// Gets installed product from it's name
        /// </summary>
        /// <param name="name">Product Name for lookup</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromProductName(string name);

        /// <summary>
        /// Gets installed product from it's product id
        /// </summary>
        /// <param name="productCode">Product guid string such as 
        /// {6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B}</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromProductCode(string productCode);

        /// <summary>
        /// Returns product name list based on lookup criteria
        /// </summary>
        /// <returns>Product name list</returns>
        IEnumerable<string> GetProductNameList();

        /// <summary>
        /// Checks if product installation exists at given path
        /// </summary>
        /// <param name="installPath">Installation path</param>
        /// <returns>true for success</returns>
        bool ExistsAtPath(string installPath);

        /// <summary>
        /// Returns full path for the core file based on install location
        /// </summary>
        /// <param name="installPath">Install location</param>
        /// <returns>Full path for core file</returns>
        string GetCoreFilePathFromInstallation(string installPath);

        /// <summary>
        /// Gets file version info from the given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Version info as Tuple</returns>
        Tuple<int, int, int, int> GetVersionInfoFromFile(string filePath);

    }

    /// <summary>
    /// Represents collection of installed productss
    /// </summary>
    public interface IProductCollection
    {
        /// <summary>
        /// Gets list of installed products
        /// </summary>
        IEnumerable<IInstalledProduct> Products { get; }

        /// <summary>
        /// Gets latest product from installation
        /// </summary>
        /// <returns>Installed product</returns>
        IInstalledProduct GetLatestProduct();

        /// <summary>
        /// Gets all installed product on the system using the given lookUp 
        /// and initializes itself.
        /// </summary>
        /// <param name="lookUp">LookUp interface</param>
        void LookUpAndInitProducts(IProductLookUp lookUp);
    }

    /// <summary>
    /// Implements basic look up algorithm to get product installations using registry keys
    /// </summary>
    public class InstalledProductLookUp : IProductLookUp
    {
        const string REG_KEY64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        const string REG_KEY32 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
        
        /// <summary>
        /// Product name for lookup
        /// </summary>
        public string ProductLookUpName { get; private set; }
        private readonly string fileLookup;

        static RegistryKey OpenKey(string key)
        {
            var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            return regKey.OpenSubKey(key);
        }

        static string GetInstallLocation(RegistryKey key)
        {
            if (key != null)
                return key.GetValue("InstallLocation") as string;

            return string.Empty;
        }

        static string GetDisplayName(RegistryKey key)
        {
            if (key != null)
                return key.GetValue("DisplayName") as string;

            return string.Empty;
        }

        /// <summary>
        /// Implements a product look up algorithm based on registry key.
        /// </summary>
        /// <param name="lookUpName">Product name to lookup</param>
        /// <param name="fileLookup">file name pattern to lookup</param>
        public InstalledProductLookUp(string lookUpName, string fileLookup)
        {
            this.ProductLookUpName = lookUpName;
            this.fileLookup = fileLookup;
        }

        public IInstalledProduct GetProductFromInstallPath(string path)
        {
            return GetProductFromInstallPath(path, string.Empty);
        }

        private InstalledProduct GetProductFromInstallPath(string path, string productName)
        {
            var product = ExistsAtPath(path)
                ? new InstalledProduct(path, this)
                : null;

            if (null != product && !string.IsNullOrEmpty(productName))
                product.ProductName = productName;

            return product;
        }

        public IInstalledProduct GetProductFromProductName(string name)
        {
            var path = GetInstallLocationFromProductName(name);
            return GetProductFromInstallPath(path, name);
        }

        public IInstalledProduct GetProductFromProductCode(string productCode)
        {
            string prodName;
            var path = GetInstallLocationFromProductCode(productCode, out prodName);
            return GetProductFromInstallPath(path, prodName);
        }

        public virtual IEnumerable<string> GetProductNameList()
        {
            var key = OpenKey(REG_KEY64);
            return key.GetSubKeyNames().Where(s => s.Contains(ProductLookUpName));
        }

        public virtual bool ExistsAtPath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return false;

            return Directory.Exists(basePath) && Directory.GetFiles(basePath, fileLookup).Any();
        }

        public virtual string GetInstallLocationFromProductName(string name)
        {
            var key = OpenKey(REG_KEY64 + name);
            return GetInstallLocation(key);
        }

        public virtual string GetInstallLocationFromProductCode(string productCode, out string productName)
        {
            string issProdKey = REG_KEY32 + productCode + "_is1";
            var key = OpenKey(issProdKey);
            if (null == key)
            {
                issProdKey = REG_KEY64 + productCode;
                key = OpenKey(issProdKey);
            }
            productName = GetDisplayName(key);
            return GetInstallLocation(key);
        }

        public virtual string GetCoreFilePathFromInstallation(string installPath)
        {
            return Directory.GetFiles(installPath, fileLookup).First();
        }

        public virtual Tuple<int, int, int, int> GetVersionInfoFromFile(string filePath)
        {
            var version = FileVersionInfo.GetVersionInfo(filePath);
            return Tuple.Create(version.FileMajorPart, version.FileMinorPart, version.FileBuildPart, version.FilePrivatePart);
        }
    }

    class InstalledProduct : IInstalledProduct
    {
        public string ProductName { get; set; }
        public string InstallLocation { get; set; }
        public string VersionString { get; set; }
        public Tuple<int, int, int, int> VersionInfo { get; private set; }

        public InstalledProduct(string installLocation, InstalledProductLookUp lookUp)
        {
            InstallLocation = installLocation;
            var corePath = lookUp.GetCoreFilePathFromInstallation(InstallLocation);
            VersionInfo = lookUp.GetVersionInfoFromFile(corePath);
            ProductName = string.Format("{0} {1}.{2}", lookUp.ProductLookUpName, VersionInfo.Item1, VersionInfo.Item2);
            VersionString = string.Format("{0}.{1}.{2}.{3}", VersionInfo.Item1, VersionInfo.Item2, VersionInfo.Item3, VersionInfo.Item4);
        }

        public int CompareTo(object obj)
        {
            var product = obj as IInstalledProduct;
            if (null == product)
                return -100000;

            var x = (VersionInfo.Item1 - product.VersionInfo.Item1);
            if (x != 0)
                return x*10000;

            x = (VersionInfo.Item2 - product.VersionInfo.Item2);
            if (x != 0)
                return x*100;

            return (VersionInfo.Item3 - product.VersionInfo.Item3);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return VersionInfo.GetHashCode();
        }
    }

    public class InstalledProducts : IProductCollection
    {
        public IEnumerable<IInstalledProduct> Products { get; protected set; }

        public IInstalledProduct GetLatestProduct()
        {
            return Products.LastOrDefault();
        }

        public virtual void LookUpAndInitProducts(IProductLookUp lookUp)
        {
            Products =
                lookUp.GetProductNameList()
                    .Select(lookUp.GetProductFromProductName).Distinct()
                    .Where(p => p != null).OrderBy(p => p);
        }
    }


    public class DynamoProducts : InstalledProducts
    {
        const string PRODUCT_ID07_X = @"{6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B}";
        const string DYNAMO063 = @"C:\Autodesk\Dynamo\Core";
        static readonly string dynamo07X = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dynamo 0.7");
        private readonly string debugPath;
        
        DynamoProducts(string debugPath)
        {
            this.debugPath = debugPath;
        }

        public static DynamoProducts FindDynamoInstallations(string debugPath = null, IProductLookUp lookUp = null)
        {
            var products = new DynamoProducts(debugPath);
            products.LookUpAndInitProducts(lookUp ?? new InstalledProductLookUp("Dynamo", "*DynamoCore.dll"));
            return products;
        }

        public override void LookUpAndInitProducts(IProductLookUp lookUp)
        {
            var products = new List<IInstalledProduct>();
            var debugProduct = lookUp.GetProductFromInstallPath(debugPath);

            products.AddRange(
                LookUpDynamoProducts(lookUp).Distinct()
                    .Where(p => p != null && p.CompareTo(debugProduct) != 0));
            
            if (null != debugProduct)
                products.Add(debugProduct);

            products.Sort();
            Products = products;
        }

        private static IEnumerable<IInstalledProduct> LookUpDynamoProducts(IProductLookUp lookUp)
        {
            yield return lookUp.GetProductFromInstallPath(DYNAMO063); //Look up 0.6.3
            yield return lookUp.GetProductFromInstallPath(dynamo07X); //
            yield return lookUp.GetProductFromProductCode(PRODUCT_ID07_X);
            foreach (var product in lookUp.GetProductNameList())
                yield return lookUp.GetProductFromProductName(product);
        }
    }
}
