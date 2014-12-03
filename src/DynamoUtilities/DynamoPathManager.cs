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
        private List<string> addResolvePaths = new List<string>();

        private static DynamoPathManager instance;

        /// <summary>
        /// The main execution path of Dynamo. This is the directory
        /// which contains DynamoCore.dll
        /// </summary>
        public string MainExecPath { get; set; }

        /// <summary>
        /// The definitions folder, which contains custom nodes
        /// created by the user.
        /// </summary>
        public string UserDefinitions { get; set; }

        /// <summary>
        /// The definitions folder which contains custom nodes
        /// available to all users.
        /// </summary>
        public string CommonDefinitions { get; set; }

        /// <summary>
        /// The location of the Samples folder.
        /// </summary>
        public string CommonSamples { get; set; }

        /// <summary>
        /// The packages folder, which contains pacakages downloaded
        /// with the package manager.
        /// </summary>
        public string Packages { get; set; }

        /// <summary>
        /// The UI folder, which contains the UI resources.
        /// </summary>
        public string Ui { get; set; }

        /// <summary>
        /// The ASM folder which contains LibG and the 
        /// ASM binaries.
        /// </summary>
        public string LibG { get; private set; }

        /// <summary>
        /// All 'nodes' folders.
        /// </summary>
        public HashSet<string> Nodes { get; set; }

        /// <summary>
        /// Libraries to be preloaded by library services.
        /// </summary>
        public IEnumerable<string> PreloadLibraries
        {
            get { return preloadLibaries; }
        }

        /// <summary>
        /// The Logs folder.
        /// </summary>
        public string Logs { get; set; }

        /// <summary>
        /// The Dynamo folder in AppData
        /// </summary>
        public string AppData { get; set;}

        public string GeometryFactory { get; set; }

        public string AsmPreloader { get; set; }

        /// <summary>
        /// Additional paths that should be searched during
        /// assembly resolution
        /// </summary>
        public List<string> AdditionalResolutionPaths
        {
            get { return addResolvePaths; }
            set { addResolvePaths = value; }
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

            Logs = Path.Combine(AppData, "Logs");
            if (!Directory.Exists(Logs))
            {
                Directory.CreateDirectory(Logs);
            }

            UserDefinitions = Path.Combine(AppData, "definitions");
            if (!Directory.Exists(UserDefinitions))
            {
                Directory.CreateDirectory(UserDefinitions);
            }

            Packages = Path.Combine(AppData, "packages");
            if (!Directory.Exists(Packages))
            {
                Directory.CreateDirectory(Packages);
            }

            var commonData = GetDynamoCommonDataFolder(MainExecPath);

            CommonDefinitions = Path.Combine(commonData, "definitions");
            if (!Directory.Exists(CommonDefinitions))
            {
                Directory.CreateDirectory(CommonDefinitions);
            }

            CommonSamples = Path.Combine(commonData, "samples");
            if (!Directory.Exists(CommonSamples))
            {
                Directory.CreateDirectory(CommonSamples);
            }

            Ui = Path.Combine(MainExecPath , "UI");

            if (Nodes == null)
            {
                Nodes = new HashSet<string>();
            }

            // Only register the core nodes directory
            Nodes.Add(Path.Combine(MainExecPath, "nodes"));

#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("MainExecPath: {0}", MainExecPath));
            sb.AppendLine(String.Format("Definitions: {0}", UserDefinitions));
            sb.AppendLine(String.Format("Packages: {0}", Packages));
            sb.AppendLine(String.Format("Ui: {0}", Ui));
            sb.AppendLine(String.Format("Asm: {0}", LibG));
            Nodes.ToList().ForEach(n=>sb.AppendLine(String.Format("Nodes: {0}", n)));
            
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

        private static string GetDynamoCommonDataFolder(string basePath)
        {
            var dynCore = Path.Combine(basePath, "DynamoCore.dll");
            var fvi = FileVersionInfo.GetVersionInfo(dynCore);
            var dynVersion = String.Format("{0}.{1}", fvi.FileMajorPart, fvi.FileMinorPart);
            var progData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Dynamo",
                dynVersion);
            return progData;
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

        public void SetLibGPath(string version)
        {
            LibG = Path.Combine(MainExecPath, string.Format("libg_{0}", version));
            var splits = LibG.Split('\\');
            GeometryFactory = splits.Last() + "\\" + "LibG.ProtoInterface.dll";
            AsmPreloader = Path.Combine(
                MainExecPath,
                splits.Last() + "\\" + "LibG.AsmPreloader.Managed.dll");

            if (!AdditionalResolutionPaths.Contains(LibG))
            {
                AdditionalResolutionPaths.Add(LibG);
            }
        }

        /// <summary>
        /// Searches the user's computer for a suitable Autodesk host application containing ASM DLLs
        /// for the specified version.
        /// </summary>
        /// <param name="version"> The version of ASM which you would like to find. Ex. "219" or "220"</param>
        /// <param name="host"></param>
        /// <returns>True if it finds the specified ASM version on the user's machine, false if it does not.</returns>
        private static bool FindAsm(string version, out string host)
        {
            host = null;

            string baseSearchDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk");

            DirectoryInfo root;

            try
            {
                root = new DirectoryInfo(baseSearchDirectory);
            }
            catch (Exception)
            {
                return false;
            }

            DirectoryInfo[] subDirs;

            try
            {
                subDirs = root.GetDirectories();
            }
            catch (Exception)
            {
                // TODO: figure out how to print to the console that Sandbox needs higher permissions
                return false;
            }

            if (subDirs.Length == 0)
                return false;

            foreach (
                var dirInfo in
                    from dirInfo in
                        subDirs.Where(
                            dirInfo => dirInfo.Name.Contains("Revit") || dirInfo.Name.Contains("Vasari"))
                    let files = dirInfo.GetFiles("*.*")
                    where files.Any(fi => fi.Name.ToUpper() == string.Format("ASMAHL{0}A.DLL", version))
                    select dirInfo)
            {
                host = dirInfo.FullName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Preload a specific version of ASM.
        /// </summary>
        /// <param name="version">The version as ex. "219"</param>
        /// <param name="pathManager"></param>
        public static bool PreloadAsmVersion(string version, DynamoPathManager pathManager)
        {
            Debug.WriteLine(string.Format("Attempting to preload ASM version {0}", version));

            string hostLocation;
            if (!FindAsm(version, out hostLocation))
            {
                Debug.WriteLine(string.Format("Could not load ASM version {0}", version));
                return false;
            }

            pathManager.SetLibGPath(version);

            var libG = Assembly.LoadFrom(Instance.AsmPreloader);

            Type preloadType = libG.GetType("Autodesk.LibG.AsmPreloader");

            MethodInfo preloadMethod = preloadType.GetMethod(
                "PreloadAsmLibraries",
                BindingFlags.Public | BindingFlags.Static);

            if (preloadMethod == null)
                throw new MissingMethodException(@"Method ""PreloadAsmLibraries"" not found");

            var methodParams = new object[1];
            methodParams[0] = hostLocation;

            preloadMethod.Invoke(null, methodParams);

            Debug.WriteLine(string.Format("Successfully loaded ASM version {0}", version));
            return true;
        }

        /// <summary>
        /// Searches the user's computer for a suitable Autodesk host application containing ASM DLLs,
        /// determines the correct version of ASM and loads the binaries.
        /// </summary>
        public static bool PreloadAsmLibraries(DynamoPathManager pathManager)
        {
            if (PreloadAsmVersion("219", pathManager)) return true;
            if (PreloadAsmVersion("220", pathManager)) return true;
            
            return false;
        }
    }
}
