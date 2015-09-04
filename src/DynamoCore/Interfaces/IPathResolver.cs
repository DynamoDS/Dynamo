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

        /// <summary>
        /// This property represents the root folder where user specific data files 
        /// are stored. If this property returns a null or empty string, then 
        /// PathManager falls back to using "%ProgramData%\Dynamo". If this property
        /// returns a string that does not represent an existing folder, PathManager 
        /// will attempt to create a new directory. If the property does not represent
        /// a valid path string, an exception will be thrown by the underlying system 
        /// IO API invoked. Note that this path should not include the version number 
        /// as it will be appended by PathManager.
        /// </summary>
        string UserDataRootFolder { get; }

        /// <summary>
        /// This property represents the root folder where application common data 
        /// files (i.e. shared among all users on the same machine) are stored. If 
        /// this property returns a null or empty string, then PathManager falls 
        /// back to using "%AppData%\Dynamo". If this property returns a string 
        /// that does not represent an existing folder, PathManager will attempt 
        /// to create a new directory. If the property does not represent a valid 
        /// path string, an exception will be thrown by the underlying system IO 
        /// API invoked. Note that this path should not include the version number 
        /// as it will be appended by PathManager.
        /// </summary>
        string CommonDataRootFolder { get; }
    }

    public interface IPathManager
    {
        /// <summary>
        /// The directory in which DynamoCore.dll is guaranteed to be found.
        /// </summary>
        string DynamoCoreDirectory { get; }

        /// <summary>
        /// The local directory that contains user specific data files.
        /// </summary>
        string UserDataDirectory { get; }

        /// <summary>
        /// The local directory that contains common data files among users.
        /// </summary>
        string CommonDataDirectory { get; }

        /// <summary>
        /// The default directory that contains custom nodes created by the user.
        /// </summary>
        string DefaultUserDefinitions { get; }

        /// <summary>
        /// Directories from where custom nodes are to be loaded. The implementor
        /// of this interface method should always guarantee that a non-empty 
        /// list is returned, and that the first entry represents the default 
        /// custom node directory. Custom nodes created are stored in the
        /// default directory, which is specific to the current user. Changes to
        /// custom nodes may or may not be saved to their current location depeding
        /// on write access.
        /// </summary>
        IEnumerable<string> DefinitionDirectories { get; }

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
        /// The default directory for saving packages downloaded through
        /// the package manager. This directory is specific to the current user.
        /// </summary>
        string DefaultPackagesDirectory { get; }

        /// <summary>
        /// Directories from where packages are to be loaded. The implementor
        /// of this interface method should always guarantee that a non-empty 
        /// list is returned, and that the first entry represents the default 
        /// package directory. Packages downloaded through package manager are 
        /// stored in the default package directory, which is specific to the 
        /// current user.
        /// </summary>
        IEnumerable<string> PackagesDirectories { get; }

        /// <summary>
        /// The directory, which contains ExtensionDefinition .xml files
        /// </summary>
        string ExtensionsDirectory { get; }

        /// <summary>
        /// The directory, which contains ViewExtensionDefinition.xml files
        /// </summary>
        string ViewExtensionsDirectory { get; }

        /// <summary>
        /// The root directory where all sample files are stored. This directory
        /// is common to all users on the machine.
        /// </summary>
        string SamplesDirectory { get; }

        /// <summary>
        /// The directory where the automatically saved files will be stored.
        /// </summary>
        string BackupDirectory { get; }

        /// <summary>
        /// Full path to the preference xml file. This setting file is specific 
        /// to the current user.
        /// </summary>
        string PreferenceFilePath { get; }

        /// <summary>
        /// Full path to the GalleryContent xml file. The file is located in
        /// the AppData/Dynamo/version/locale/
        /// </summary>
        string GalleryFilePath { get; }

        /// <summary>
        /// Folders in which node assemblies can be located.
        /// </summary>
        IEnumerable<string> NodeDirectories { get; }

        /// <summary>
        /// A list of node assembly names to be preloaded with Dynamo.
        /// </summary>
        IEnumerable<string> PreloadedLibraries { get; }

        /// <summary>
        /// Major version of assembly file
        /// </summary>
        int MajorFileVersion { get; }

        /// <summary>
        /// Minor version of assembly file
        /// </summary>
        int MinorFileVersion { get; }

        /// <summary>
        /// Call this method to add additional path for consideration when path 
        /// resolution take place.
        /// </summary>
        /// <param name="path">The full path to be considered when PathManager
        /// attempt to resolve a file path. If this argument does not represent 
        /// a valid directory path, this method throws an exception.</param>
        void AddResolutionPath(string path);

        /// <summary>
        /// Given an initial file path with the file name, resolve the full path
        /// to the target file.
        /// </summary>
        /// <param name="library">The initial library file path. This argument 
        /// can optionally include the full path with a target file name. If a 
        /// full path is given and it represents an invalid file path, the file 
        /// name will be searched for in additional resolution paths.</param>
        /// <returns>Returns true if the requested file can be located, or false
        /// otherwise.</returns>
        bool ResolveLibraryPath(ref string library);

        /// <summary>
        /// Given an initial RTF document file name, this method returns the 
        /// absolute path of the file, if one exists.
        /// </summary>
        /// <param name="document">The name of the RTF file. This argument cannot 
        /// be null or empty.</param>
        /// <returns>Returns true if the requested document can be located, or 
        /// false otherwise.</returns>
        bool ResolveDocumentPath(ref string document);
    }
}
