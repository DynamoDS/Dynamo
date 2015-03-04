using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public interface IPathResolver
    {
        /// <summary>
        /// Additional directories that should be considered when path resolution
        /// is done for a library that does not contain full path information. 
        /// The return value of this property should never be null. Each entry 
        /// must represent a valid directory, otherwise DirectoryNotFoundException
        /// exception is thrown.
        /// </summary>
        IEnumerable<string> AdditionalResolutionPaths { get; }

        /// <summary>
        /// Additional directories in which node assemblies can be located. The 
        /// return value of this property should never be null. Each entry must 
        /// represent a valid directory, otherwise DirectoryNotFoundException
        /// exception is thrown.  
        /// </summary>
        IEnumerable<string> AdditionalNodeDirectories { get; }

        /// <summary>
        /// Libraries to be preloaded as part of Dynamo start up sequence. Each
        /// entry in this list can either represent full path to a library, or 
        /// just the assembly name. If absolute path information is not supplied,
        /// the library will be looked up through both predefined and additional 
        /// resolution paths. The return value of this property should never be 
        /// null.
        /// </summary>
        IEnumerable<string> PreloadedLibraryPaths { get; }
    }

    public interface IPathManager
    {
        /// <summary>
        /// The local directory that contains custom nodes created by the user.
        /// </summary>
        string UserDefinitions { get; }

        /// <summary>
        /// The local directory that contains custom nodes created by all users.
        /// </summary>
        string CommonDefinitions { get; }

        /// <summary>
        /// The local directory where log files are generated. This directory is 
        /// specific to the current user.
        /// </summary>
        string LogDirectory { get; }

        /// <summary>
        /// The packages directory, which contains pacakages downloaded through
        /// the package manager. This directory is specific to the current user.
        /// </summary>
        string PackagesDirectory { get; }

        /// <summary>
        /// The root directory where all sample files are stored. This directory
        /// is common to all users on the machine.
        /// </summary>
        string SamplesDirectory { get; }

        /// <summary>
        /// Full path to the preference xml file. This setting file is specific 
        /// to the current user.
        /// </summary>
        string PreferenceFilePath { get; }

        IEnumerable<string> NodeDirectories { get; }
    }
}
