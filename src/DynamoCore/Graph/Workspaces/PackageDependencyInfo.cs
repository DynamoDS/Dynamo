using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Class containing info about a Dynamo package. 
    /// Used for serialization.
    /// </summary>
    internal class PackageDependencyInfo
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        internal Version Version { get; set; }

        /// <summary>
        /// The full name of this package, including its name and version
        /// Format: name_version
        /// </summary>
        internal string FullName { get { return Name + "_" + Version.ToString(); } }

        /// <summary>
        /// Guids of nodes in the workspace that are dependent on this package
        /// </summary>
        internal List<Guid> Dependents
        {
            get { return dependents; }
        }
        private List<Guid> dependents;
        
        /// <summary>
        /// Create a package info object from the package name and version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        internal PackageDependencyInfo(string name, Version version)
        {
            Name = name;
            Version = version;
            dependents = new List<Guid>();
        }

        /// <summary>
        /// Add the Guid of a dependent node
        /// </summary>
        /// <param name="guid"></param>
        internal void AddDependent(Guid guid)
        {
            Dependents.Add(guid);
        }
    }
}
