using System;
using System.Collections.Generic;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Class containing info about a package
    /// </summary>
    public class PackageInfo
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
        /// Create a package info object from the package name and version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        internal PackageInfo(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Checks whether two PackageInfos are equal
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
            if (!(obj is PackageInfo))
            {
                return false;
            }

            var other = obj as PackageInfo;
            if (other.Name == this.Name && other.Version == this.Version)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the hashcode for this PackageInfo
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        /// <summary>
        /// Get the string representing this PackageInfo
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + ", Version=" + Version.ToString();
        }
    }

    internal enum ReferenceType{
        NodeModel,
        Package,
        ZeroTouch,
        DSFile,
        //TODO - This is already covered by the older Dependencies property
        DYFFILE
    }



    /// <summary>
    /// An interface that describes a dependency a workspace can have on other code.
    /// </summary>
    interface INodeLibraryDependencyInfo
    {
        /// <summary>
        /// The type of reference this dependency is.
        /// </summary>
        ReferenceType ReferenceType { get; }

        /// <summary>
        /// Guids of nodes in the workspace that are dependent on this reference.
        /// </summary>
        HashSet<Guid> Nodes { get; }

        /// <summary>
        /// Name of the Reference.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of this reference. This may be null.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Add the Guid of a dependent node
        /// </summary>
        /// <param name="guid"></param>
        void AddDependent(Guid guid);

        /// <summary>
        /// Indicates whether this dependency is loaded in the current session
        /// </summary>
        bool IsLoaded { get; set; }
    }

    /// <summary>
    /// Class containing info about a workspace package dependency
    /// </summary>
    internal class PackageDependencyInfo : INodeLibraryDependencyInfo
    {
        /// <summary>
        /// PackageInfo for this package
        /// </summary>
        internal PackageInfo PackageInfo { get; set; }

        /// <summary>
        /// Name of the package
        /// </summary>
        public string Name => PackageInfo.Name;

        /// <summary>
        /// Version of the package
        /// </summary>
        public Version Version => PackageInfo.Version;

        /// <summary>
        /// Indicates whether this package is loaded in the current session
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Guids of nodes in the workspace that are dependent on this package
        /// </summary>
        public HashSet<Guid> Nodes
        {
            get { return nodes; }
        }

        public ReferenceType ReferenceType => ReferenceType.Package;

        private HashSet<Guid> nodes;
        
        /// <summary>
        /// Create a package dependency from the package name and version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        internal PackageDependencyInfo(string name, Version version)
        {
            PackageInfo = new PackageInfo(name, version);
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Create a package dependency from its package info
        /// </summary>
        /// <param name="packageInfo"></param>
        internal PackageDependencyInfo(PackageInfo packageInfo)
        {
            PackageInfo = packageInfo;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Add the Guid of a dependent node
        /// </summary>
        /// <param name="guid"></param>
        public void AddDependent(Guid guid)
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
        /// Checks whether two PackageDependencyInfo instances are equal
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

        /// <summary>
        /// Gets the hashcode for this PackageDependencyInfo
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        /// <summary>
        /// Get the string representing this PackageDependencyInfo
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + ", Version=" + Version.ToString();
        }
    }
}
