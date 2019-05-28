using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Class containing info about a Dynamo package. 
    /// Used for serialization.
    /// </summary>
    public class PackageInfo
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Create a package info object from the package name, version, and assembly names of assemblies contained in the package
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        public PackageInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
