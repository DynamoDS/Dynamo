using Dynamo.Models;
using Dynamo.PackageManager;
using System.Collections.Generic;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    public interface IPackageManagerCommands
    {
        DynamoModel Model { get; set; }

        PackageLoader Loader { get; set; }

        IEnumerable<Package> LocalPackages { get; }

        /// <summary>
        /// Install Dynamo Package
        /// </summary>
        /// <param name="downloadPath"></param>
        void LoadPackage(Package package);

        /// <summary>
        /// Uninstall Dynamo Package
        /// </summary>
        /// <returns></returns>
        bool UnloadPackage(Package localPackage);

        MessageBoxResult ShowMessageBox(MessageTypes msgID, string msg, string caption, MessageBoxButton options, MessageBoxImage boxImage);

    }
}
