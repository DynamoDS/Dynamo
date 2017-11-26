using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    public class RenderPackageCache
    {
        private List<IRenderPackage> packages;
        private Dictionary<Guid, List<IRenderPackage>> portMap;

        private void AddPort(IRenderPackage package, Guid outputPortId)
        {
            if (!portMap.ContainsKey(outputPortId))
            {
                portMap[outputPortId] = new List<IRenderPackage>();
            }

            portMap[outputPortId].Add(package);
        }

        // TODO, QNTM-2631: The packages list returned should take into account the
        // output port they were created by if needed
        public IEnumerable<IRenderPackage> Packages
        {
            get
            {
                return packages;
            }
        }

        public RenderPackageCache()
        {
            packages = new List<IRenderPackage>();
            portMap = new Dictionary<Guid, List<IRenderPackage>>();
        }

        // TODO, QNTM-2631: This should include the GUIDs of the output ports that the packages came from
        public RenderPackageCache(IEnumerable<IRenderPackage> otherPackages)
        : this()
        {
            packages.AddRange(otherPackages);
        }

        public bool IsEmpty()
        {
            return packages.Count == 0;
        }

        public void Add(RenderPackageCache other)
        {
            packages.AddRange(other.packages);
            foreach(var port in other.portMap)
            {
                foreach(var item in port.Value)
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
