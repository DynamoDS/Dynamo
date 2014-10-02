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
                string uninstallPath;
                if (!GetUninstallPathFromRegistry(session, out uninstallPath))
                {
                    return ActionResult.NotExecuted;
                }

                if (!string.IsNullOrEmpty(uninstallPath) && File.Exists(uninstallPath))
                {
                    Uninstall(session, uninstallPath);
                }
                else
                {
                    session.Log(string.Format("Dynamo uninstall path: {0}, could not be located.", uninstallPath));
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
    }
}
