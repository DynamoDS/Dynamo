using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    public interface IRenderPackageSource<T>
    {
        /// <summary>
        /// An event raised then the source has updated IRenderPackages.
        /// </summary>
        event Action<T, IEnumerable<IRenderPackage>> RenderPackagesUpdated;
    }
}
