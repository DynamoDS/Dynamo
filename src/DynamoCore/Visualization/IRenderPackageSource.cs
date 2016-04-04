using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRenderPackageSource<T>
    {
        /// <summary>
        /// An event raised then the source has updated IRenderPackages.
        /// </summary>
        event Action<T, IEnumerable<IRenderPackage>> RenderPackagesUpdated;
    }
}
