using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
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

        public RenderPackageCache()
        {
            packages = new List<IRenderPackage>();
            portMap = null;
        }

        public RenderPackageCache(IEnumerable<IRenderPackage> otherPackages)
        : this()
        {
            packages.AddRange(otherPackages);
        }

        public IEnumerable<IRenderPackage> Packages
        {
            get
            {
                return packages;
            }
        }

        public RenderPackageCache GetPortPackages(Guid portId)
        {
            if (portMap == null)
                return null;

            RenderPackageCache portPackages;
            if (!portMap.TryGetValue(portId, out portPackages))
                return null;

            if (portPackages == null)
                return null;

            return portPackages;
        }

        public bool IsEmpty()
        {
            return packages.Count == 0;
        }

        public void Add(RenderPackageCache other)
        {
            packages.AddRange(other.packages);

            if (other.portMap == null)
                return;

            foreach (var port in other.portMap)
            {
                foreach(var item in port.Value.packages)
                {
                    AddPort(item, port.Key);
                }
            }
        }

        public void Add(IRenderPackage package)
        {
            packages.Add(package);
        }

        public void Add(IRenderPackage package, Guid outputPortId)
        {
            Add(package);
            AddPort(package, outputPortId);
        }
    }
}
