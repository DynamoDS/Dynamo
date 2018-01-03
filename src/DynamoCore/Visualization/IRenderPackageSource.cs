using System;

namespace Dynamo.Visualization
{
    /// <summary>
    /// This interface has events that is fired when the render packages are changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRenderPackageSource<T>
    {
        /// <summary>
        /// An event raised then the source has updated IRenderPackages.
        /// </summary>
        event Action<T, RenderPackageCache> RenderPackagesUpdated;
    }
}
