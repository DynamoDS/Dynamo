using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace DynamoInstallActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UninstallDynamo07(Session session)
        {
            session.Log("Begin Dynamo uninstall.");

            try
            {
                // Dynamo 0.7.1 AppId
                // 6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B
                string exeUninstallPath, msiUninstallPath = string.Empty;
                if (!GetUninstallPathFromRegistry(session, out exeUninstallPath) &&
                    !GetOldMSIUninstallStringFromRegistery(session, out msiUninstallPath))
                {
                    return ActionResult.NotExecuted;
                }

                if (!string.IsNullOrEmpty(exeUninstallPath) && File.Exists(exeUninstallPath))
                {
                    Uninstall(session, exeUninstallPath);
                }
                else if (!string.IsNullOrEmpty(msiUninstallPath) && File.Exists(msiUninstallPath))
                {
                    UninstallOldMSI(session, msiUninstallPath);
                }
                else
                {
                    session.Log(string.Format("Dynamo uninstall path: {0}, could not be located.", exeUninstallPath));
                    return ActionResult.NotExecuted;
                }

                return ActionResult.Success;

            }
            catch (Exception ex)
            {
                session.Log("There was an error uninstalling Dynamo:");
                session.Log(ex.Message);
                return ActionResult.Failure;
            }
        }

        private static void UninstallOldMSI(Session session, string uninstallPath)
        {
            session.Log(string.Format("Uninstalling Old Dynamo MSI 0.7 at {0}", uninstallPath));
            string options = uninstallPath.Remove(0, 11) + @" \quiet";
            var proc = Process.Start(uninstallPath, options);
            if (proc != null)
            {
                proc.WaitForExit();
            }
        }

        /// <summary>
        /// Run the Dynamo 0.7.1 uninstaller
        /// </summary>
        /// <param name="session"></param>
        /// <param name="uninstallPath"></param>
        private static void Uninstall(Session session, string uninstallPath)
        {
            session.Log(string.Format("Uninstalling Dynamo 0.7 at {0}", uninstallPath));
            var proc = Process.Start(uninstallPath, "/VERYSILENT");
            if (proc != null)
            {
                proc.WaitForExit();
            }

            // Delete the dynamo directory as well.
            //var uninstallDir = new DirectoryInfo(Path.GetDirectoryName(uninstallPath));
            //var dynamoDir = uninstallDir.Parent.FullName;
            //if (Directory.Exists(dynamoDir))
            //{
            //    Directory.Delete(dynamoDir);
            //}
        }

        /// <summary>
        /// Find an install of Dynamo 0.7.1 in the registry using the AppId
        /// </summary>
        /// <param name="session"></param>
        /// <param name="uninstallPath"></param>
        /// <returns></returns>
        private static bool GetUninstallPathFromRegistry(Session session, out string uninstallPath)
        {
            const string key071 =
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B}_is1";
            var key =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    RegistryView.Registry64);
            key = key.OpenSubKey(key071);

            if (key == null)
            {
                session.Log("No Dynamo installs could be located in the registry.");
                {
                    uninstallPath = string.Empty;
                    return false;
                }
            }

            var uninstallKey = key.GetValue("UninstallString");
            session.Log(string.Format("Uninstall key:{0}", uninstallKey));

            uninstallPath = Path.GetFullPath(uninstallKey.ToString().Replace(@"""", @""));
            return true;
        }

        private static bool GetOldMSIUninstallStringFromRegistery(Session session, out string uninstallString)
        {
            const string uninstallPath =
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{6B5FA6CA-9D69-46CF-B517-1F90C64F7C0B}";
            var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            key = key.OpenSubKey(uninstallPath);
            if (key == null)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(uninstallPath);
            }

            if(key == null)
            {
                session.Log("No Dynamo MSI installs could be located in the registry.");
                {
                    uninstallString = string.Empty;
                    return false;
                }
            }

            var uninstallKey = key.GetValue("UninstallString");
            session.Log(string.Format("Uninstall key:{0}", uninstallKey));

            uninstallString = Path.GetFullPath(uninstallKey.ToString().Replace(@"""", @""));
            return true;
        }
    }
}
