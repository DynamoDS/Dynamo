using System;
using System.IO;
using Microsoft.Win32;

namespace DynamoInstallDetective
{
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
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

        private const string registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\SharedComponents\";

        private readonly string majorRelease;
        private readonly string version;
        private bool fetchFromEnv;
        private string installPath;
        private string regPath;

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

            var registryPath = Path.Combine(registryKey, majorRelease);
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

        public AscSdkWrapper(string release)
        {
            majorRelease = release;
            version = @"ACS_VERSION_" + majorRelease;
        }

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
