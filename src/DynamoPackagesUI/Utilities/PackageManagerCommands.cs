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

        public MessageBoxResult Show(PackageManagerMessages msgID, string caption, MessageBoxButton options, MessageBoxImage boxImage, params object[] args)
        {

            return MessageBox.Show(GetPackageManagerMessage(msgID, args), caption, options, boxImage);
        }

        private string GetPackageManagerMessage(PackageManagerMessages msgID, params object[] args)
        {
            switch (msgID)
            {
                case PackageManagerMessages.FAIL_TO_UNINSTALL_PACKAGE: return string.Format(Resources.MessageFailToUninstallPackage, args);
                case PackageManagerMessages.CONFIRM_TO_INSTALL_PACKAGE: return string.Format(Resources.MessageConfirmToInstallPackage, args);
                case PackageManagerMessages.UNINSTALL_TO_CONTINUE: return string.Format(Resources.MessageUninstallToContinue, args);
                case PackageManagerMessages.UNINSTALL_TO_CONTINUE2: return string.Format(Resources.MessageUninstallToContinue2, args);
                case PackageManagerMessages.ALREADY_INSTALLED_DYNAMO: return string.Format(Resources.MessageAlreadyInstallDynamo, args);
                case PackageManagerMessages.PACKAGE_CONTAIN_PYTHON_SCRIPT: return string.Format(Resources.MessagePackageContainPythonScript, args);
                case PackageManagerMessages.PACKAGE_NEWER_DYNAMO: return string.Format(Resources.MessagePackageNewerDynamo, args);
                case PackageManagerMessages.NEED_TO_RESTART: return string.Format(Resources.MessageNeedToRestart, args);
                case PackageManagerMessages.CONFIRM_TO_UNINSTALL: return string.Format(Resources.MessageConfirmToUninstallPackage, args);
                case PackageManagerMessages.FAILED_TO_UNINSTALL: return string.Format(Resources.MessageFailedToUninstall, args);
                case PackageManagerMessages.CONFIRM_TO_INSTALL_PACKAGE_FOLDER: return string.Format(Resources.MessageConfirmToInstallPackageToFolder, args);
            }
            return string.Empty;
        }

    }

    public enum PackageManagerMessages
    {
        FAIL_TO_UNINSTALL_PACKAGE,
        CONFIRM_TO_INSTALL_PACKAGE,
        CONFIRM_TO_INSTALL_PACKAGE_FOLDER,
        UNINSTALL_TO_CONTINUE,
        UNINSTALL_TO_CONTINUE2,
        ALREADY_INSTALLED_DYNAMO,
        PACKAGE_CONTAIN_PYTHON_SCRIPT,
        PACKAGE_NEWER_DYNAMO,
        NEED_TO_RESTART,
        CONFIRM_TO_UNINSTALL,
        FAILED_TO_UNINSTALL
    }
}
