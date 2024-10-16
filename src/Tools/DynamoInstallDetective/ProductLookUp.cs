using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace DynamoInstallDetective
{
    // Utility class for interacting with the windows registry.
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal static class RegUtils
    {
        // Utility class to enable/disable registry caching within a scope.
        class RegistryCacher : IDisposable
        {
            public RegistryCacher()
            {
                lock (mutex)
                {
                    cacheEnabled = true;
                }
            }

            public void Dispose()
            {
                lock (mutex)
                {
                    cacheEnabled = false;
                    cachedRecords?.Clear();
                    cachedRecords = null;
                }
            }
        }

        public static string REG_KEY64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        public static string REG_KEY32 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";

        // Cached data from windows registry. Consists of a dictionary that maps The product name/code to Display Name (Tuple.item1) and Install Path(Tuple.item2)
        // Dictionary<ProductCode, (DisplayName, InstallPath)>
        private static Dictionary<string, (string DisplayName, string InstallLocation)> cachedRecords;
        private static bool cacheEnabled = false;

        private static readonly object mutex = new object();

        public static RegistryKey OpenKey(string key)
        {
            var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            return regKey.OpenSubKey(key);
        }
        public static string GetInstallLocation(RegistryKey key)
        {
            if (key != null)
                return key.GetValue("InstallLocation") as string;

            return string.Empty;
        }

        public static string GetDisplayName(RegistryKey key)
        {
            if (key != null)
                return key.GetValue("DisplayName") as string;

            return string.Empty;
        }

        
        /// <summary>
        /// Returns all the products registered under "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" that have a valid DisplayName and an InstallLocation.
        /// NOTE! Because this static method returns a static field that might be cleaned up by the owning RegistryCacher don't use this method in deffered queries
        /// unless you force a copy to be made using ToList(), ToArray() or some other immediate query.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, (string DisplayName, string InstallLocation)> GetInstalledProducts()
        {
            lock (mutex)
            {
                if (cacheEnabled && cachedRecords != null)
                {
                    return cachedRecords;
                }
            }

            var productInfo = new Dictionary<string, (string displayName, string installLocation)>();

            var key = OpenKey(REG_KEY64);
            foreach(string s in key.GetSubKeyNames())
            {
                try
                {
                    var prodKey = key.OpenSubKey(s);
                    var displayName = GetDisplayName(prodKey);
                    var installLocation = GetInstallLocation(prodKey);
                    if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(installLocation))
                    {
                        productInfo.Add(s, (displayName, installLocation));
                    }
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }

            lock (mutex)
            {
                if (cacheEnabled)
                {
                    cachedRecords = new Dictionary<string, (string DisplayName, string InstallLocation)>(productInfo);
                }
            }
            return productInfo;
        }

        public static IDisposable StartCache()
        {
            return new RegistryCacher();
        }
    }

    /// <summary>
    /// Specifies an installed product
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
        /// Returns installed product from the installation path
        /// </summary>
        /// <param name="path">Installation path</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromInstallPath(string path);

        /// <summary>
        /// Returns installed product from it's name
        /// </summary>
        /// <param name="name">Product Name for lookup</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromProductName(string name);

        /// <summary>
        /// Returns installed product from it's product id
        /// </summary>
        /// <param name="productCode">Product guid string such as 
        /// {6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B}</param>
        /// <returns>Installed product</returns>
        IInstalledProduct GetProductFromProductCode(string productCode);

        /// <summary>
        /// Returns product name list using lookup criteria based on product names.
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
        /// Returns file version info from the given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Version info as Tuple</returns>
        Tuple<int, int, int, int> GetVersionInfoFromFile(string filePath);

    }

    /// <summary>
    /// Represents collection of installed products
    /// </summary>
    public interface IProductCollection
    {
        /// <summary>
        /// Returns list of installed products
        /// </summary>
        IEnumerable<IInstalledProduct> Products { get; }

        /// <summary>
        /// Returns latest product from installation
        /// </summary>
        /// <returns>Installed product</returns>
        IInstalledProduct GetLatestProduct();

        /// <summary>
        /// Get all installed products on the system using the given lookUp 
        /// and append to Products property.
        /// </summary>
        /// <param name="lookUp">LookUp interface</param>
        void LookUpAndInitProducts(IProductLookUp lookUp);
    }

    /// <summary>
    /// Implements basic look up algorithm to get product installations using registry keys
    /// </summary>
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public class InstalledProductLookUp : IProductLookUp
    {
        /// <summary>
        /// Product name for lookup
        /// </summary>
        public string ProductLookUpName { get; private set; }

        private readonly Func<string, string> fileLocator;

        /// <summary>
        /// Implements a product look up algorithm based on registry key.
        /// </summary>
        /// <param name="lookUpName">Product name to lookup</param>
        /// <param name="fileLookup">file name pattern to lookup</param>
        public InstalledProductLookUp(string lookUpName, string fileLookup)
        {
            ProductLookUpName = lookUpName;
            fileLocator = (path) => Directory.EnumerateFiles(path, fileLookup, SearchOption.AllDirectories)
                .FirstOrDefault();
        }

        public InstalledProductLookUp(string lookUpName, Func<string, string> fileLocator)
        {
            this.ProductLookUpName = lookUpName;
            this.fileLocator = fileLocator;
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
            return RegUtils.GetInstalledProducts().ToList().Select(s => s.Value.DisplayName).Where(s => {
                return s?.Contains(ProductLookUpName) ?? false;
            });
        }
        //TODO add to IProductLookUp interface in Dynamo 3.0
        //Returns product names and code tuples for products which have valid display name.
        internal virtual IEnumerable<(string DisplayName, string ProductKey)> GetProductNameAndCodeList()
        {
            return RegUtils.GetInstalledProducts().ToList().Select(s => (s.Value.DisplayName,s.Key)).Where(s => {
                return s.DisplayName?.Contains(ProductLookUpName) ?? false;
            });
        }

        public virtual bool ExistsAtPath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return false;

            return Directory.Exists(basePath) && File.Exists(fileLocator(basePath));
        }

        public virtual string GetInstallLocationFromProductName(string name)
        {
            var key = RegUtils.OpenKey(RegUtils.REG_KEY64 + name);
            return RegUtils.GetInstallLocation(key);
        }

        public virtual string GetInstallLocationFromProductCode(string productCode, out string productName)
        {
            string issProdKey = RegUtils.REG_KEY32 + productCode + "_is1";
            var key = RegUtils.OpenKey(issProdKey);
            if (null == key)
            {
                issProdKey = RegUtils.REG_KEY64 + productCode;
                key = RegUtils.OpenKey(issProdKey);
            }
            productName = RegUtils.GetDisplayName(key);
            return RegUtils.GetInstallLocation(key);
        }

        public virtual string GetCoreFilePathFromInstallation(string installPath)
        {
            return fileLocator(installPath);
        }

        public virtual Tuple<int, int, int, int> GetVersionInfoFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return Tuple.Create(0, 0, 0, 0);
            var version = FileVersionInfo.GetVersionInfo(filePath);
            return Tuple.Create(version.FileMajorPart, version.FileMinorPart, version.FileBuildPart, version.FilePrivatePart);
        }
    }

#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    ///
    /// Helper class for looking up the install directories for all installed ASC components
    ///
    public class InstalledAscLookUp : InstalledProductLookUp
    {
        const string asc = @"Autodesk Shared Components";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileLookup">File to look for</param>
        public InstalledAscLookUp(string fileLookup) : base(asc, fileLookup)
        {
        }

        /// <summary>
        /// Get all major ASC versions
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable<(string DisplayName, string ProductKey)> GetProductNameAndCodeList()
        {
            var list = AscSdkWrapper.GetMajorVersions().Select(x => (DisplayName: x, ProductKey: string.Empty));
            return list;
        }

        /// <summary>
        /// The result is never used but still needs to be overridden
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetProductNameList()
        {
            var list = new List<string>();
            return list;
        }

        /// <summary>
        /// Get the install location for the ASC component
        /// </summary>
        /// <param name="name">ASC major verion</param>
        /// <returns></returns>
        public override string GetInstallLocationFromProductName(string name)
        {
            AscSdkWrapper asc = new AscSdkWrapper(name);
            string path = string.Empty;
            if(asc.GetInstalledPath(ref path) == AscSdkWrapper.ASC_STATUS.SUCCESS)
            {
                return path;
            }

            return null;
        }
    }

#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    class InstalledProduct : IInstalledProduct
    {
        public string ProductName { get; set; }
        public string InstallLocation { get; set; }
        public string VersionString { get; set; }
        public Tuple<int, int, int, int> VersionInfo { get; private set; }

        public InstalledProduct(string installLocation, InstalledProductLookUp lookUp)
        {
            var corePath = lookUp.GetCoreFilePathFromInstallation(installLocation);
            InstallLocation = File.Exists(corePath) ? Path.GetDirectoryName(corePath) : installLocation;
            
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

#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public class InstalledProducts : IProductCollection
    {
        public IEnumerable<IInstalledProduct> Products { get; protected set; }

        public IInstalledProduct GetLatestProduct()
        {
            return Products.LastOrDefault();
        }

        public virtual void LookUpAndInitProducts(IProductLookUp lookUp)
        {
            var newProductTuples = lookUp.GetProductNameList().Select(x=>(DisplayName: x, ProductKey : string.Empty));
            if (lookUp is InstalledProductLookUp lookupAsInstalledProduct)
            {
                newProductTuples = lookupAsInstalledProduct.GetProductNameAndCodeList();
            }
          
            var returnProducts = newProductTuples.Select(prod =>
            {
                var product = lookUp.GetProductFromProductName(prod.DisplayName);
                if (product == null)
                {
                    product = lookUp.GetProductFromProductCode(prod.ProductKey);
                }
                return product;
            }).Distinct().Where(p => p != null).OrderBy(p => p);
            Products = Products == null ? returnProducts : Products.Concat(returnProducts);
        }
    }

#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
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

        public static string GetDynamoPath(Version version, string debugPath = null)
        {
            var additionalPath = debugPath;
            var configPath = Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location), "Dynamo.config");
            if (File.Exists(configPath))
            {
                // Get DynamoCore path from the Dynamo.config file, if it exists
                var map = new ExeConfigurationFileMap() { ExeConfigFilename = configPath };
                
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                var runtime = config.AppSettings.Settings["DynamoRuntime"];
                if (runtime != null && !string.IsNullOrEmpty(runtime.Value) && Directory.Exists(runtime.Value))
                {
                    additionalPath = runtime.Value;
                }
            }

            var installs = FindDynamoInstallations(additionalPath);
            if (installs == null) return string.Empty;

            return installs.Products
                .Where(p => p.VersionInfo.Item1 == version.Major)
                .Where(p => p.VersionInfo.Item2 >= version.Minor)
                .Select(p => p.InstallLocation)
                .LastOrDefault();
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
