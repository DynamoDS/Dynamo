using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// Interface that contains basic properties of a Dynamo package
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        string VersionName { get; set; }

        /// <summary>
        /// AssemblyNames of all assemblies loaded as part of this package
        /// </summary>
        IEnumerable<AssemblyName> AssemblyNames { get; }
    }
}
