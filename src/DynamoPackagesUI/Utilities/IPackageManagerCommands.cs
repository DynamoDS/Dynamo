using Dynamo.Models;
using Dynamo.PackageManager;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    public interface IPackageManagerCommands
    {
        DynamoModel Model { get; set; }

        PackageLoader Loader { get; set; }

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

    }
}
