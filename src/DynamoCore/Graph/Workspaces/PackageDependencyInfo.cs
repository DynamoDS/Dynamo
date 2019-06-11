using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Internals are visible to the Package Manager extension
// For workspace package dependency collection
[assembly: InternalsVisibleTo("PackageDependencyViewExtension")]


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
        /// Guids of nodes in the workspace that are dependent on this package
        /// </summary>
        internal HashSet<Guid> Nodes
        {
            get { return nodes; }
        }
        private HashSet<Guid> nodes;
        
        /// <summary>
        /// Create a package info object from the package name and version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        internal PackageDependencyInfo(string name, Version version)
        {
            Name = name;
            Version = version;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Add the Guid of a dependent node
        /// </summary>
        /// <param name="guid"></param>
        internal void AddDependent(Guid guid)
        {
             Nodes.Add(guid);
        }

        /// <summary>
        /// Add the Guids of a dependent nodes
        /// </summary>
        /// <param name="guids"></param>
        internal void AddDependents(IEnumerable<Guid> guids)
        {
            foreach(var guid in guids)
            {
                Nodes.Add(guid);
            }
        }

        /// <summary>
        /// Remove a dependent node
        /// </summary>
        /// <param name="guid"></param>
        internal void RemoveDependent(Guid guid)
        {
            Nodes.Remove(guid);
        }

        /// <summary>
        /// Checks whether two PackageDependencyInfos are equal
        /// They are equal if their Name and Versions are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PackageDependencyInfo))
            {
                return false;
            }

            var other = obj as PackageDependencyInfo;
            if (other.Name == this.Name && other.Version == this.Version)
            {
                return true;
            }
            return false;
        }
        //TODO override GetHashCode
    }
}
