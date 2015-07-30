using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Interfaces
{
    public interface IRenderPackageSource
    {
        /// <summary>
        /// An event raised then the source has updated IRenderPackages.
        /// </summary>
        event Action<Guid, IEnumerable<IRenderPackage>> RenderPackagesUpdated;
    }
}
