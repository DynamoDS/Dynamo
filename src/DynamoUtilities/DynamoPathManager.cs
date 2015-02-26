using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DynamoUtilities
{
    /// <summary>
    /// DynamoPathManager stores paths to dynamo libraries and assets.
    /// </summary>
    public class DynamoPathManager
    {
        private readonly HashSet<string> preloadLibaries = new HashSet<string>();
        private readonly List<string> addResolvePaths = new List<string>();

        private static DynamoPathManager instance;

        /// <summary>
        /// The main execution path of Dynamo. This is the directory
        /// which contains DynamoCore.dll
        /// </summary>
        public string MainExecPath { get; private set; }

        /// <summary>
        /// Libraries to be preloaded by library services.
        /// </summary>
        public IEnumerable<string> PreloadLibraries
        {
            get { return preloadLibaries; }
        }

        /// <summary>
        /// The Dynamo folder in AppData
        /// </summary>
        public string AppData { get; private set;}

        /// <summary>
        /// Additional paths that should be searched during
        /// assembly resolution
        /// </summary>
        public List<string> AdditionalResolutionPaths
        {
            get { return addResolvePaths; }
        }

        public static DynamoPathManager Instance
        {
            get { return instance ?? (instance = new DynamoPathManager()); }
        }

        internal static void DestroyInstance()
        {
            instance = null;
        }

        /// <summary>
        /// Provided a main execution path, find other Dynamo paths
        /// relatively. This operation should be called only once at
        /// the beginning of a Dynamo session.
        /// </summary>
        /// <param name="mainExecPath">The main execution directory of Dynamo.</param>
        public void InitializeCore(string mainExecPath)
        {
            if (Directory.Exists(mainExecPath))
            {
                MainExecPath = mainExecPath;
            }
            else
            {
                throw new Exception(String.Format("The specified main execution path: {0}, does not exist.", mainExecPath));
            }

            AppData = GetDynamoAppDataFolder(MainExecPath);

#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("MainExecPath: {0}", MainExecPath));            
            Debug.WriteLine(sb);
#endif
            var coreLibs = new List<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "Optimize.ds",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll"
            };

            foreach (var lib in coreLibs)
            {
                AddPreloadLibrary(lib);
            }
        }

        private static string GetDynamoAppDataFolder(string basePath)
        {
            var dynCore = Path.Combine(basePath, "DynamoCore.dll");

            if (!File.Exists(dynCore))
            {
                throw new Exception("Dynamo's core path could not be found. If you are running Dynamo from a test, try specifying the Dynamo core location in the DynamoBasePath variable in TestServices.dll.config.");
            }

            var fvi = FileVersionInfo.GetVersionInfo(dynCore);
            var dynVersion = String.Format("{0}.{1}", fvi.FileMajorPart, fvi.FileMinorPart);
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dynamo",
                dynVersion);
            return appData;
        }

        /// <summary>
        /// Given an initial file path with the file name, resolve the full path
        /// to the target file. The search happens in the following order:
        /// 
        /// 1. Alongside DynamoCore.dll folder (i.e. the "Add-in" folder).
        /// 2. The AdditionalResolutionPaths
        /// 3. System path resolution.
        /// 
        /// </summary>
        /// <param name="library">The initial library file path.</param>
        /// <returns>Returns true if the requested file can be located, or false
        /// otherwise.</returns>
        public bool ResolveLibraryPath(ref string library)
        {
            if (File.Exists(library)) // Absolute path, we're done here.
                return true;

            library = LibrarySearchPaths(library).FirstOrDefault(File.Exists);
            return library != default(string);
        }

        private IEnumerable<string> LibrarySearchPaths(string library)
        {
            string assemblyName = Path.GetFileName(library); // Strip out possible directory.
            if (assemblyName == null)
                yield break;

            var assemPath = Path.Combine(Instance.MainExecPath ?? "", assemblyName);
            yield return assemPath;

            foreach (var path in AdditionalResolutionPaths.Select(dir => Path.Combine(dir, assemblyName)))
                yield return path;

            yield return Path.GetFullPath(library);
        }

        /// <summary>
        /// Add a library for preloading with a check.
        /// </summary>
        /// <param name="path"></param>
        public void AddPreloadLibrary(string path)
        {
            if (!preloadLibaries.Contains(path))
                preloadLibaries.Add(path);
        }

        /// <summary>
        /// Adds a library for resolution with a check.
        /// </summary>
        /// <param name="path"></param>
        public void AddResolutionPath(string path)
        {
            if (!addResolvePaths.Contains(path))
                addResolvePaths.Add(path);
        }
    }
}
