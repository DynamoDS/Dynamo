using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                if (!preloadLibaries.Contains(lib))
                    preloadLibaries.Add(lib);
            }
        }
    }
}
