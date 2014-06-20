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
    /// DynamoPaths stores paths to dynamo libraries and assets.
    /// </summary>
    public static class DynamoPaths
    {
        private static List<string> preloadLibaries = new List<string>();
        private static List<string> addResolvePaths = new List<string>();

        /// <summary>
        /// The main execution path of Dynamo. This is the directory
        /// which contains DynamoCore.dll
        /// </summary>
        public static string MainExecPath { get; set; }

        /// <summary>
        /// The definitions folder, which contains custom nodes
        /// created by the user.
        /// </summary>
        public static string Definitions { get; set; }

        /// <summary>
        /// The packages folder, which contains pacakages downloaded
        /// with the package manager.
        /// </summary>
        public static string Packages { get; set; }

        /// <summary>
        /// The UI folder, which contains the UI resources.
        /// </summary>
        public static string Ui { get; set; }

        /// <summary>
        /// The ASM folder which contains LibG and the 
        /// ASM binaries.
        /// </summary>
        public static string Asm { get; set; }

        // All 'nodes' folders.
        public static HashSet<string> Nodes { get; set; }

        /// <summary>
        /// Libraries to be preloaded by library services.
        /// </summary>
        public static List<string> PreloadLibraries
        {
            get { return preloadLibaries; }
            set { preloadLibaries = value; }
        }

        /// <summary>
        /// Additional paths that should be searched during
        /// assembly resolution
        /// </summary>
        public static List<string> AdditionalResolutionPaths
        {
            get { return addResolvePaths; }
            set { addResolvePaths = value; }
        }

        /// <summary>
        /// Provided a main execution path, find other Dynamo paths
        /// relatively. This operation should be called only once at
        /// the beginning of a Dynamo session.
        /// </summary>
        /// <param name="mainExecPath">The main execution directory of Dynamo.</param>
        /// <param name="preloadLibraries">A list of libraries to preload.</param>
        public static void SetupDynamoPathsCore(string mainExecPath)
        {
            if (Directory.Exists(mainExecPath))
            {
                MainExecPath = mainExecPath;
            }
            else
            {
                throw new Exception(string.Format("The specified main execution path: {0}, does not exist.", mainExecPath));
            }

            var appData = GetDynamoAppDataFolder(MainExecPath);

            Definitions = Path.Combine(appData, "definitions");
            Packages = Path.Combine(appData, "packages");

            if (!Directory.Exists(Definitions))
            {
                Directory.CreateDirectory(Definitions);
            }

            if (!Directory.Exists(Packages))
            {
                Directory.CreateDirectory(Packages);
            }

            Asm = Path.Combine(MainExecPath, "dll");
            Ui = Path.Combine(MainExecPath , "UI");

            if (Nodes == null)
            {
                Nodes = new HashSet<string>();
            }

            // Only register the core nodes directory
            Nodes.Add(Path.Combine(MainExecPath, "nodes"));

#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("MainExecPath: {0}", MainExecPath));
            sb.AppendLine(string.Format("Definitions: {0}", Definitions));
            sb.AppendLine(string.Format("Packages: {0}", Packages));
            sb.AppendLine(string.Format("Ui: {0}", Asm));
            sb.AppendLine(string.Format("Asm: {0}", Ui));
            Nodes.ToList().ForEach(n=>sb.AppendLine(string.Format("Nodes: {0}", n)));
            
            Debug.WriteLine(sb);
#endif
            var coreLibs = new List<string>
            {
                "ProtoGeometry.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "Optimize.ds",
                "DynamoUnits.dll",
                "Tessellation.dll"
            };

            foreach (var lib in coreLibs)
            {
                AddPreloadLibrary(lib);
            }
        }

        private static string GetDynamoAppDataFolder(string basePath)
        {
            var dynCore = Path.Combine(basePath, "DynamoCore.dll");
            var fvi = FileVersionInfo.GetVersionInfo(dynCore);
            var dynVersion = string.Format("{0}.{1}", fvi.FileMajorPart, fvi.FileMinorPart);
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dynamo",
                dynVersion);
            return appData;
        }

        /// <summary>
        /// Add a library for preloading with a check.
        /// </summary>
        /// <param name="path"></param>
        public static void AddPreloadLibrary(string path)
        {
            if (!preloadLibaries.Contains(path))
            {
                preloadLibaries.Add(path);
            }
        }

        /// <summary>
        /// Adds a library for resolution with a check.
        /// </summary>
        /// <param name="path"></param>
        public static void AddResolutionPath(string path)
        {
            if (!addResolvePaths.Contains(path))
            {
                addResolvePaths.Add(path);
            }
        }
    }
}
