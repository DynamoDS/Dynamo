using Dynamo.Graph.Workspaces;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// An interface containing operations for installing Dynamo packages
    /// </summary>
    public interface IPackageInstaller
    {
        /// <summary>
        /// Initiates download and install of a package
        /// </summary>
        /// <param name="package">Package Info of the package to be downloaded--includes package name and version</param>
        /// <param name="downloadPath">Path to download location of the package</param>
        void DownloadAndInstallPackage(IPackageInfo package, string downloadPath = null);
    }
}
