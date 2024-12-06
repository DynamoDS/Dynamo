using System;
using System.IO;
using Microsoft.Win32;

namespace DynamoInstallDetective
{
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    ///
    /// Autodesk Shared Components SDK
    ///
    /// There is one or more installed shared components packages for each major version.
    /// Major versions are for example 2026, 2027 and 2028 and corresponds to our global launches.
    /// Minor versions are for example 1.0.0, 1.1.0, 2.0.0 etc.
    ///
    /// These packages are ususally installed in "C:\Program Files\Common Files\Autodesk Shared\Components", 
    /// for example in C:\Program Files\Common Files\Autodesk Shared\Components\2026\1.0.0 for version 1.0.0 of the package used by 2026 products.
    ///
    /// There is no guarantee that this localtion is used as it can be overridden at deplyments time. Instead, the registry should be queried
    /// for the final location.
    ///
    /// The registry location "HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\SharedComponents\" contains one key for each major version,
    /// for example the key 2026 is used for the 2026 package.
    ///
    /// Each key will hold two values:
    /// Version: The current version of the package
    /// InstallPath: The actual location where the package is installed.
    ///
    /// There is usually only one minor version installed for each major version but there are situations when multiple minor versions can be installed.
    /// One of these cases is when testing beta versions of the shared components. The version used can then be overridden by using an environment variable,
    /// ACS_VERSION_<majorRelease> that points to the prefered version to use. So to load the 2.0-beta verion of the 2026 components you would
    /// set ACS_VERSION_2026 to 2.0-beta.
    ///
    /// All shared components are currenly installed into one flat directory for each release. Possible future changes include the ability to only install those
    /// components that actually is needed. This wrapper might need to be updated at that point.
    /// 
    internal class AscSdkWrapper
    {
        public enum ASC_STATUS
        {
            SUCCESS = 0,
            UNABLE_TO_SET_INSTALL_PATH = 4,
            BAD_ARGS = 5,
            REG_FAILED = 6,
            FUNCTION_CALL_FAILED = 7,
            INITIALIZE_FAILED = 8,
            ODIS_SDK_INITIALIZE_FAILED = 9,
            ODIS_SDK_LOCK_FAILED = 10,
            ODIS_SDK_UNLOCK_FAILED = 11,
            INCORRECT_REG_PATH = 12,
            EMPTY_REG_VERSION = 13,
            FAILED = 100
        };

        private const string registryKey = @"SOFTWARE\Autodesk\SharedComponents\";

        private readonly string majorRelease;
        private readonly string version;
        private bool fetchFromEnv;
        private string installPath;
        private string regPath;

        /// <summary>
        /// Allows the basekey to be ovrridden, used by unit tests
        /// </summary>
        public RegistryKey BaseKey { get; set; } = Registry.LocalMachine;

        private ASC_STATUS ReadASCVersionFromEnv()
        {
            ASC_STATUS status = ASC_STATUS.FAILED;

            var minorVersion = Environment.GetEnvironmentVariable(version);

            if(minorVersion != null)
            {
                fetchFromEnv = true;

                status = ReadASCInstallPathFromRegistry(majorRelease);

                installPath = Path.Combine(installPath, minorVersion);

                if(status != ASC_STATUS.SUCCESS)
                {
                    fetchFromEnv = false;
                }
            }

            return status;
        }

        private ASC_STATUS ReadASCInstallPathFromRegistry(string majorRelease)
        {
            ASC_STATUS status = ASC_STATUS.REG_FAILED;

            var registryPath = Path.Combine(BaseKey.Name, registryKey, majorRelease);
            if(string.IsNullOrEmpty(regPath))
            {
                regPath = registryPath;
            }

            const string valueName = @"InstallPath";
            const string versionName = @"Version";

            string installPath = Registry.GetValue(registryPath, valueName, null) as String;
            if (installPath != null)
            {
                if (!Directory.Exists(installPath))
                {
                    return ASC_STATUS.INCORRECT_REG_PATH;
                }

                if (fetchFromEnv)
                {
                    this.installPath = installPath;
                    status = ASC_STATUS.SUCCESS;
                }
                else
                {
                    string version = Registry.GetValue(registryPath, versionName, null) as String;
                    if (string.IsNullOrEmpty(version))
                    {
                        return ASC_STATUS.EMPTY_REG_VERSION;
                    }

                    this.installPath = Path.Combine(installPath, version);

                    if (!Directory.Exists(this.installPath))
                    {
                        return ASC_STATUS.INCORRECT_REG_PATH;
                    }
                    status = ASC_STATUS.SUCCESS;
                }
            }
            return status;
        }
        /// <summary>
        /// Initialize ASC wrapper
        /// </summary>
        /// <param name="release">Major ASC release, for example 2026</param>
        public AscSdkWrapper(string release)
        {
            majorRelease = release;
            version = @"ACS_VERSION_" + majorRelease;
        }

        /// <summary>
        /// Get the install path for the major release
        /// </summary>
        /// <param name="installedPath">The install path for the major release if successful</param>
        /// <returns>ASC_STATUS.SUCCESS if path was retrived successfully</returns>
        public ASC_STATUS GetInstalledPath(ref string installedPath)
        {
            if(string.IsNullOrEmpty(majorRelease))
            {
                return ASC_STATUS.INITIALIZE_FAILED;
            }

            var status = ReadASCVersionFromEnv() == ASC_STATUS.SUCCESS ? ASC_STATUS.SUCCESS : ReadASCInstallPathFromRegistry(majorRelease);
            installedPath = installPath;

            return status;
        }

        /// <summary>
        /// Get the major version of all ASC packages installed on the local machine
        /// </summary>
        /// <returns>An array of major versions, for example ["2026, "2027", "2028"]</returns>
        public static string[] GetMajorVersions()
        {
            string[] majorVersions = [];
            var baseKey = Registry.LocalMachine;
            var subkey = baseKey.OpenSubKey(@"SOFTWARE\Autodesk\SharedComponents");
            if(subkey != null)
            {
                majorVersions = subkey.GetSubKeyNames();
                subkey.Close();
            }
            return majorVersions;
        }
    }
}
