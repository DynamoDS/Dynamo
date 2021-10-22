using System;
using System.Collections.Generic;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Enum containing the different types of package dependency states.
    /// </summary>
    public enum PackageDependencyState
    {
        Loaded,            // Correct package and version loaded.
        IncorrectVersion,  // Correct package but incorrect version. 
        Missing,           // package is completely missing.
        Warning,           // Actual package is missing but the nodes are resolved by some other package. 
        RequiresRestart    // Restart needed in order to complete the uninstall of some package. Notice this would be only set when workspace references extension is loaded.
    }

    /// <summary>
    /// Interface for types containing info about a package
    /// </summary>
    public interface IPackageInfo
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the package
        /// </summary>
        Version Version { get; }
    }

    /// <summary>
    /// Class containing info about a package
    /// </summary>
    public class PackageInfo : IPackageInfo
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        public Version Version { get; internal set; }

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

    /// <summary>
    /// Class containing info about a LocalDefinition or an External File
    /// </summary>
    internal class DependencyInfo: INodeLibraryDependencyInfo
    {
        /// <summary>
        /// Name of the Dependency
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Path of the Dependency
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Size of the Dependency
        /// </summary>
        public string Size { get; internal set; }

        /// <summary>
        /// ReferenceType of the Dependency
        /// </summary>
        public ReferenceType ReferenceType { get; internal set; }

        private HashSet<Guid> nodes;

        public HashSet<Guid> Nodes
        {
            get { return nodes; }
        }

        public Version Version { get; internal set; }

        /// <summary>
        /// Indicates whether this Dependency is loaded in the current session
        /// </summary>
        [Obsolete("This property is obsolete", false)]
        public bool IsLoaded { get; set; }

        public PackageDependencyState State { get; internal set; }

        /// <summary>
        /// Create a Dependency info object from the Name and Path
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        internal DependencyInfo(string name, string path)
        {
            Name = name;
            Path = path;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Create a Dependency info object from the Name, Path and ReferenceType
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="referenceType"></param>
        internal DependencyInfo(string name, string path, ReferenceType referenceType)
        {
            Name = name;
            Path = path;
            ReferenceType = referenceType;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Create a Dependency info object from the Name and ReferenceType
        /// </summary>
        /// <param name="name"></param>
        /// <param name="referenceType"></param>
        internal DependencyInfo(string name, ReferenceType referenceType)
        {
            Name = name;
            ReferenceType = referenceType;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Create a Dependency info object from the name
        /// </summary>
        /// <param name="name"></param>
        internal DependencyInfo(string name)
        {
            Name = name;
            nodes = new HashSet<Guid>();
        }

        /// <summary>
        /// Checks whether two Dependency's are equal
        /// They are equal if their Name, Path and ReferenceType are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is DependencyInfo))
            {
                return false;
            }

            var other = obj as DependencyInfo;
            if (other.Name == this.Name && other.Path == this.Path && other.ReferenceType == this.ReferenceType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the hashcode for this dependency
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Size.GetHashCode();
        }

        /// <summary>
        /// Get the string representing this dependency.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + ", Size=" + Size;
        }

        public void AddDependent(Guid guid)
        {
            Nodes.Add(guid);
        }
    }

    internal enum ReferenceType
    {
        NodeModel,
        Package,
        ZeroTouch,
        DSFile,
        DYFFile,
        External
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
        [Obsolete("This property is obsolete", false)]
        bool IsLoaded { get; set; }

        /// <summary>
        /// The state of this dependency
        /// </summary>
        PackageDependencyState State { get; }
    }

    /// <summary>
    /// Class containing info about a workspace package dependency
    /// </summary>
    internal class PackageDependencyInfo : INodeLibraryDependencyInfo, IPackageInfo
    {
        private PackageDependencyState _state;
        /// <summary>
        /// PackageInfo for this package
        /// </summary>
        internal PackageInfo PackageInfo { get; set; }

        /// <summary>
        /// Name of the package
        /// </summary>
        public string Name => PackageInfo.Name;

        /// <summary>
        /// Package path.
        /// </summary>
        public String Path { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        public Version Version
        {
            get
            {
                return PackageInfo.Version;
            }
            internal set
            {
                if(PackageInfo.Version != value)
                    PackageInfo.Version = value;
            }
        }

        /// <summary>
        /// Indicates whether this package is loaded in the current session
        /// </summary>
        [Obsolete("This property is obsolete, use State property instead", false)]
        public bool IsLoaded{ get; set;}

        /// <summary>
        /// State of Package Dependency
        /// </summary>
        public PackageDependencyState State {
            
            get {
                return _state;
            } 
            set {
                _state = value;
                if (_state == PackageDependencyState.Loaded) {
                    this.IsLoaded = true;
                }
            }
        }

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
