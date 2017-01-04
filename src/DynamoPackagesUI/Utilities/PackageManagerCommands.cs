using Dynamo.DynamoPackagesUI.Properties;
using Dynamo.Models;
using Dynamo.PackageManager;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    /// <summary>
    /// CEF calss to assist exploring packages, authors and logged in user packages.
    /// </summary>
    internal class PackageManagerCommands : IPackageManagerCommands
    {
        
        public DynamoModel Model { get; set; }

        public PackageLoader Loader { get; set; }

        public IEnumerable<Package> LocalPackages { get { return Loader.LocalPackages; } }

        
        public PackageManagerCommands(PackageLoader loader, DynamoModel model) 
        {
            this.Loader = loader;
            this.Model = model;
        }

        /// <summary>
        /// Install Dynamo Package
        /// </summary>
        /// <param name="downloadPath"></param>
        public void LoadPackage(Package package)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                Loader.Load(package);
            }));
        }

        /// <summary>
        /// Uninstall Dynamo Package
        /// </summary>
        /// <returns></returns>
        public bool UnloadPackage(Package localPackage)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                localPackage.UninstallCore(Model.CustomNodeManager, Loader, Model.PreferenceSettings);
            }));

            return true;
        }

        public MessageBoxResult ShowMessageBox(MessageTypes msgID, string msg, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return MessageBox.Show(msg, caption, options, boxImage);
        }

    }

    public enum MessageTypes
    {
        FailToUninstallPackage,
        ConfirmToInstallPackage,
        ConfirmToInstallPackageFolder,
        UnistallToContinue,
        UnistallToContinue2,
        AlreadyInstalledDynamo,
        PackageContainPythinScript,
        PackageNewerDynamo,
        NeedToRestart,
        ConfirmToUninstall,
        FailedToUninstall
    }
}
