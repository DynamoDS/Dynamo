﻿using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace DynamoInstallActions
{
    public class CustomActions
    {

        [CustomAction]
        public static ActionResult UninstallDynamo(Session session)
        {
            session.Log("Begin Dynamo uninstall.");

            try
            {
                string UninstallString;
                string UninstallParam;
                session.Log("Before checking");
                if (!GetUninstallString(session, out UninstallString, out UninstallParam))
                {
                    return ActionResult.NotExecuted;
                }

                if (!string.IsNullOrEmpty(UninstallString) && File.Exists(UninstallString))
                {
                    Uninstall(session, UninstallString, UninstallParam);
                }
                else
                {
                    session.Log(string.Format("Dynamo uninstall path: {0}, could not be located.", UninstallString));
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
        /// Run the Dynamo uninstaller
        /// </summary>
        /// <param name="session"></param>
        /// <param name="UninstallString"></param>
        /// <param name="UninstallParam"></param>
        private static void Uninstall(Session session, string UninstallString, string UninstallParam)
        {
            session.Log(string.Format("Uninstalling Dynamo at {0}", UninstallString));
            var proc = Process.Start(UninstallString, UninstallParam);
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
        /// Find an install of Dynamo in the registry
        /// </summary>
        /// <param name="session"></param>
        /// <param name="UninstallString"></param>
        /// <param name="UninstallParam"></param>
        /// <returns></returns>
        private static bool GetUninstallString(Session session, out string UninstallString, out string UninstallParam)
        {
            string pdt = string.Format("Dynamo {0}.{1}", session.CustomActionData["Major"], session.CustomActionData["Minor"]);
            string keyPath = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0}", pdt);
            session.Log(string.Format("Product Name {0}", pdt));
            session.Log(string.Format("KeyPath {0}", keyPath));

            var key =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    RegistryView.Registry64);
            key = key.OpenSubKey(keyPath);

            if (key == null)
            {
                session.Log("No Dynamo installs could be located in the registry.");
                {
                    UninstallString = string.Empty;
                    UninstallParam = string.Empty;
                    return false;
                }
            }

            int installedRev = int.Parse(key.GetValue("RevVersion").ToString());
            if (installedRev > int.Parse(session.CustomActionData["Rev"]))
            {
                session.Log("The installed Dynamo version is higher than this one.");
                {
                    UninstallString = string.Empty;
                    UninstallParam = string.Empty;
                    return false;
                }
            }

            UninstallString = key.GetValue("UninstallString").ToString();
            UninstallParam = key.GetValue("UninstallParam").ToString();
            session.Log(string.Format("UninstallString:{0}", UninstallString));
            session.Log(string.Format("UninstallParam:{0}", UninstallString));
            return true;
        }
    }
}
