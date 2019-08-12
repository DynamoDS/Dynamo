using System.Collections.Generic;
using Dynamo.PackageManager;

namespace DynamoPackages.Interfaces
{
    public interface IPackageLoader
    {
        IEnumerable<Package> LocalPackages { get; }
    }
}
