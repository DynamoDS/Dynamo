using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Interfaces;

namespace Dynamo.Applications
{
    internal class SandboxPathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public SandboxPathResolver(string preloaderLocation)
        {
            // If a suitable preloader cannot be found on the system, then do 
            // not add invalid path into additional resolution. The default 
            // implementation of IPathManager in Dynamo insists on having valid 
            // paths specified through "IPathResolver" implementation.
            // 
            additionalResolutionPaths = new List<string>();
            if (Directory.Exists(preloaderLocation))
                additionalResolutionPaths.Add(preloaderLocation);

            additionalNodeDirectories = new List<string>();
            preloadedLibraryPaths = new List<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DesignScriptBuiltin.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSCPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll",
                "GeometryColor.dll"
            };

        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder
        {
            get { return Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
                "Dynamo", "Dynamo Core").ToString(); }
        }

        public string CommonDataRootFolder
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Returns the full path of user data location of all version of this
        /// Dynamo product installed on this system. The default implementation
        /// returns list of all subfolders in %appdata%\Dynamo as well as 
        /// %appdata%\Dynamo\Dynamo Core\ folders.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDynamoUserDataLocations()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dynamoFolder = Path.Combine(appDatafolder, "Dynamo");
            if (!Directory.Exists(dynamoFolder)) return Enumerable.Empty<string>();

            var paths = new List<string>();
            var coreFolder = new FileInfo(UserDataRootFolder).FullName;
            //Dynamo Core folder has to be enumerated first to cater migration from
            //Dynamo 1.0 to Dynamo Core 1.0
            if (Directory.Exists(coreFolder))
            {
                paths.AddRange(Directory.EnumerateDirectories(coreFolder));
            }

            paths.AddRange(Directory.EnumerateDirectories(dynamoFolder));
            return paths;
        }
    }

    internal class CLIPathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public CLIPathResolver(string preloaderLocation, string userDataFolder, string commonDataFolder)
        {
            // If a suitable preloader cannot be found on the system, then do 
            // not add invalid path into additional resolution. The default 
            // implementation of IPathManager in Dynamo insists on having valid 
            // paths specified through "IPathResolver" implementation.
            // 
            additionalResolutionPaths = new List<string>();
            if (Directory.Exists(preloaderLocation))
                additionalResolutionPaths.Add(preloaderLocation);

            additionalNodeDirectories = new List<string>();

            preloadedLibraryPaths = new List<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DesignScriptBuiltin.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSCPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll",
                "GeometryColor.dll"
            };

            UserDataRootFolder = userDataFolder;
            CommonDataRootFolder = commonDataFolder;
        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder { get; private set; }

        public string CommonDataRootFolder { get; private set; }

        public IEnumerable<string> GetDynamoUserDataLocations()
        {
            // Do nothing for now.
            return Enumerable.Empty<string>();
        }
    }
}
