using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private const string m_registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\SharedComponents\";

        private readonly string m_majorRelease;
        private readonly string m_Version;
        private bool m_fetchFromEnv;
        private string m_installPath;
        private string m_regPath;

        private ASC_STATUS ReadASCVersionFromEnv()
        {
            ASC_STATUS status = ASC_STATUS.FAILED;

            var minorVersion = Environment.GetEnvironmentVariable(m_Version);

            if(minorVersion != null)
            {
                m_fetchFromEnv = true;

                status = ReadASCInstallPathFromRegistry(m_majorRelease);

                m_installPath = Path.Combine(m_installPath, minorVersion);

                if(status != ASC_STATUS.SUCCESS)
                {
                    m_fetchFromEnv = false;
                }
            }

            return status;
        }

        private ASC_STATUS ReadASCInstallPathFromRegistry(string majorRelease)
        {
            ASC_STATUS status = ASC_STATUS.REG_FAILED;

            var registryPath = Path.Combine(m_registryKey, m_majorRelease);
            if(string.IsNullOrEmpty(m_regPath))
            {
                m_regPath = registryPath;
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

                if (m_fetchFromEnv)
                {
                    m_installPath = installPath;
                    status = ASC_STATUS.SUCCESS;
                }
                else
                {
                    string version = Registry.GetValue(registryPath, versionName, null) as String;
                    if (string.IsNullOrEmpty(version))
                    {
                        return ASC_STATUS.EMPTY_REG_VERSION;
                    }

                    m_installPath = Path.Combine(installPath, version);

                    if (!Directory.Exists(m_installPath))
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
            m_majorRelease = release;
            m_Version = @"ACS_VERSION_" + m_majorRelease;
        }

        public ASC_STATUS GetInstalledPath(ref string installedPath)
        {
            if(string.IsNullOrEmpty(m_majorRelease))
            {
                return ASC_STATUS.INITIALIZE_FAILED;
            }

            var status = ReadASCVersionFromEnv() == ASC_STATUS.SUCCESS ? ASC_STATUS.SUCCESS : ReadASCInstallPathFromRegistry(m_majorRelease);
            installedPath = m_installPath;

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
