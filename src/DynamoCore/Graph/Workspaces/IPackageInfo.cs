using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Interface for types containing info about a package
    /// </summary>
    public interface IPackageInfo
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        Version Version { get; set; }
    }
}
