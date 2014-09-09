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
        public enum Asm{ Version219,Version220}

        private List<string> preloadLibaries = new List<string>();
        private List<string> addResolvePaths = new List<string>();
        private Asm asmVersion = Asm.Version219;
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
        public List<string> PreloadLibraries
        {
            get { return preloadLibaries; }
            set { preloadLibaries = value; }
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
        /// A directory containing the ASM 219 DLLs
        /// </summary>
        public string ASM219Host { get; set; }

        /// <summary>
        /// A directory containing the ASM 220 DLLs
        /// </summary>
        public string ASM220Host { get; set; }

        /// <summary>
        /// Additional paths that should be searched during
        /// assembly resolution
        /// </summary>
        public List<string> AdditionalResolutionPaths
        {
            get { return addResolvePaths; }
            set { addResolvePaths = value; }
        }

        public Asm ASMVersion
        {
            get { return asmVersion; }
            set { asmVersion = value; }
        }

        public static DynamoPathManager Instance
        {
            get { return instance ?? (instance = new DynamoPathManager()); }
        }

        /// <summary>
        /// Provided a main execution path, find other Dynamo paths
        /// relatively. This operation should be called only once at
        /// the beginning of a Dynamo session.
        /// </summary>
        /// <param name="mainExecPath">The main execution directory of Dynamo.</param>
        /// <param name="preloadLibraries">A list of libraries to preload.</param>
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

            switch (asmVersion)
            {
                case Asm.Version219:
                    SetLibGPath(Path.Combine(MainExecPath, "libg_219"));
                    break;
                case Asm.Version220:
                    SetLibGPath(Path.Combine(MainExecPath, "libg_220"));
                    break;
                default:
                    SetLibGPath(Path.Combine(MainExecPath, "libg_219"));
                    break;
            }

            ASM219Host = null;
            ASM220Host = null;

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

            // Give add-in folder a higher priority and look alongside "DynamoCore.dll".
            string assemblyName = Path.GetFileName(library); // Strip out possible directory.
            var assemPath = Path.Combine(Instance.MainExecPath ?? "", assemblyName);

            if (File.Exists(assemPath)) // Found under add-in folder...
            {
                library = assemPath;
                return true;
            }

            foreach (var dir in AdditionalResolutionPaths)
            {
                var path = Path.Combine(dir, assemblyName);
                if (!File.Exists(path)) continue;
                library = path;
                return true;
            }

            library = Path.GetFullPath(library); // Fallback on system search.
            return File.Exists(library);
        }

        /// <summary>
        /// Add a library for preloading with a check.
        /// </summary>
        /// <param name="path"></param>
        public void AddPreloadLibrary(string path)
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
        public void AddResolutionPath(string path)
        {
            if (!addResolvePaths.Contains(path))
            {
                addResolvePaths.Add(path);
            }
        }

        public void SetLibGPath(string path)
        {
            LibG = path;
            var splits = LibG.Split('\\');
            GeometryFactory = splits.Last() + "\\" + "LibG.ProtoInterface.dll";
            AsmPreloader = splits.Last() + "\\" + "LibG.AsmPreloader.Managed.dll";
        }

        /// <summary>
        /// Searches the user's computer for a suitable Autodesk host application containing ASM DLLs
        /// </summary>
        /// <returns>True if it finds a directory, false if it can't find a directory</returns>
        private bool FindAndSetASMHostPath()
        {
            string baseSearchDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk");

            DirectoryInfo root = null;

            try
            {
                root = new DirectoryInfo(baseSearchDirectory);
            }
            catch (Exception e)
            {
                // TODO: print to console

                return false;
            }

            FileInfo[] files;
            DirectoryInfo[] subDirs;

            try
            {
                subDirs = root.GetDirectories();
            }
            catch (Exception e)
            {
                // TODO: figure out how to print to the console that Sandbox needs higher permissions
                return false;
            }

            if (subDirs.Length == 0)
                return false;

            foreach (var dirInfo in subDirs)
            {
                // AutoCAD directories don't seem to contain all the needed ASM DLLs
                if (!dirInfo.Name.Contains("Revit") && !dirInfo.Name.Contains("Vasari"))
                    continue;

                files = dirInfo.GetFiles("*.*");

                foreach (System.IO.FileInfo fi in files)
                {
                    if (fi.Name.ToUpper() == "ASMAHL219A.DLL")
                    {
                        // we found a match for the ASM 219 dir
                        ASM219Host = dirInfo.FullName;

                        break;
                    }

                    if (fi.Name.ToUpper() == "ASMAHL220A.DLL")
                    {
                        // we found a match for the ASM 220 dir
                        ASM220Host = dirInfo.FullName;

                        break;
                    }
                }

                if (ASM219Host != null && ASM220Host != null)
                    return true;
            }

            return ASM219Host != null || ASM220Host != null;
        }

        /// <summary>
        /// Searches the user's computer for a suitable Autodesk host application containing ASM DLLs,
        /// determines correct version of ASM and loads binaries
        /// </summary>
        public bool PreloadASMLibraries()
        {
            if (!DynamoPathManager.Instance.FindAndSetASMHostPath())
                return false;
            
            if (DynamoPathManager.Instance.ASM219Host == null)
            {
                DynamoPathManager.Instance.SetLibGPath("libg_220");
                DynamoPathManager.Instance.ASMVersion = DynamoPathManager.Asm.Version220;
            }

            var libG = Assembly.LoadFrom(DynamoPathManager.Instance.AsmPreloader);

            Type preloadType = libG.GetType("Autodesk.LibG.AsmPreloader");

            MethodInfo preloadMethod = preloadType.GetMethod("PreloadAsmLibraries",
                BindingFlags.Public | BindingFlags.Static);

            if(preloadMethod == null)
                throw new MissingMethodException(@"Method ""PreloadAsmLibraries"" not found");

            object[] methodParams = new object[1];

            if (DynamoPathManager.Instance.ASM219Host == null)
                methodParams[0] = DynamoPathManager.Instance.ASM220Host;
            else
                methodParams[0] = DynamoPathManager.Instance.ASM219Host;

            preloadMethod.Invoke(null, methodParams);
                
            return true;
        }
    }
}
