using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    public class RenderPackageCache
    {
        private List<IRenderPackage> packages;

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
        }

        // TODO, QNTM-2631: This should include the GUIDs of the output ports that the packages came from
        public RenderPackageCache(IEnumerable<IRenderPackage> otherPackages)
        {
            packages = new List<IRenderPackage>();
            foreach (var package in otherPackages)
            {
                packages.Add(package);
            }
        }

        public bool IsEmpty()
        {
            return packages.Count == 0;
        }

        public void Add(RenderPackageCache other)
        {
            foreach (var package in other.packages)
            {
                packages.Add(package);
            }
        }

        // TODO, QNTM-2631: This should include the GUID of the output port the package came from
        public void Add(IRenderPackage package)
        {
            packages.Add(package);
        }
    }
}
