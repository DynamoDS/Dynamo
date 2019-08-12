using System.Collections.Generic;
using Dynamo.PackageManager;

namespace DynamoPackages.Interfaces
{
    /// <summary>
    /// This interface is for PackageLoader. It can be expanded if needed,
    /// currently only exposed inViewLoadedParams and referenced in WorkspaceDependencyViewExtension
    /// </summary>
    public interface IPackageLoader
    {
        /// <summary>
        /// List of local packages
        /// </summary>
        IEnumerable<Package> LocalPackages { get; }
    }
}
