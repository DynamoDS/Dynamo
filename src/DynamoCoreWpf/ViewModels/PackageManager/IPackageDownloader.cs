using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="package"></param>
        /// <param name="downloadPath"></param>
        void DownloadAndInstallPackage(IPackageInfo package, string downloadPath = null);
    }
}
