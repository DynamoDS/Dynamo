using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    /// <summary>
    /// This class controls the collection and distribution of render packages
    /// </summary>
    public class RenderPackageCache
    {
        private List<IRenderPackage> packages;
        private Dictionary<Guid, RenderPackageCache> portMap;

        private void AddPort(IRenderPackage package, Guid outputPortId)
        {
            if (portMap == null)
            {
                portMap = new Dictionary<Guid, RenderPackageCache>();
            }

            if (!portMap.ContainsKey(outputPortId))
            {
                portMap[outputPortId] = new RenderPackageCache();
            }

            portMap[outputPortId].Add(package);
        }

        /// <summary>
        /// Create an empty RenderPackageCache object
        /// </summary>
        public RenderPackageCache()
        {
            packages = new List<IRenderPackage>();
            portMap = null;
        }

        /// <summary>
        /// Create a RenderPackageCache object containing the given render packages
        /// </summary>
        /// <param name="otherPackages">The packages to fill the new cache object.</param>
        public RenderPackageCache(IEnumerable<IRenderPackage> otherPackages)
        : this()
        {
            packages.AddRange(otherPackages);
        }

        /// <summary>
        /// Get the render packages in this cache
        /// </summary>
        public IEnumerable<IRenderPackage> Packages
        {
            get
            {
                return packages;
            }
        }

        /// <summary>
        /// Get the RenderPackageCache object for the given port ID
        /// </summary>
        /// <param name="portId">The port ID used to get the sub-cache.</param>
        public RenderPackageCache GetPortPackages(Guid portId)
        {
            if (portMap == null)
            {
                return null;
            }

            RenderPackageCache portPackages;
            if (!portMap.TryGetValue(portId, out portPackages))
            {
                return null;
            }

            return portPackages;
        }

        /// <summary>
        /// Returns true if the cache is empty
        /// </summary>
        public bool IsEmpty()
        {
            return packages.Count == 0;
        }

        /// <summary>
        /// Concatenates the other cache into this cache
        /// </summary>
        /// <param name="other">The cache to add to this cache.</param>
        public void Add(RenderPackageCache other)
        {
            packages.AddRange(other.packages);

            if (other.portMap == null)
            {
                return;
            }

            foreach (var port in other.portMap)
            {
                foreach(var item in port.Value.packages)
                {
                    AddPort(item, port.Key);
                }
            }
        }

        /// <summary>
        /// Adds a render package to this cache
        /// </summary>
        /// <param name="package">The package to add to this cache.</param>
        public void Add(IRenderPackage package)
        {
            packages.Add(package);
        }

        /// <summary>
        /// Adds a render package to this cache, including a reference to 
        /// the output port that the package originated from
        /// </summary>
        /// <param name="package">The package to add to this cache.</param>
        /// <param name="outputPortId">The output port to associate the package with.</param>
        public void Add(IRenderPackage package, Guid outputPortId)
        {
            Add(package);
            AddPort(package, outputPortId);
        }
    }
}
