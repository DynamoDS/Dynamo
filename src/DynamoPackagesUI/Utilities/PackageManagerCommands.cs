using Dynamo.Models;
using Dynamo.PackageManager;
using System;
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
        
    }
}
